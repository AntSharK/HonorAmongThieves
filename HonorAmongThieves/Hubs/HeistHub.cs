using HonorAmongThieves.Game;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HonorAmongThieves.Hubs
{
    public class HeistHub : Hub
    {
        internal async Task ShowError(string errorMessage)
        {
            await Clients.Caller.SendAsync("ShowError", errorMessage);
        }

        // To create a room
        public async Task CreateRoom(string userName)
        {
            var roomId = Program.Instance.CreateRoom(this, userName);
            if (!string.IsNullOrEmpty(roomId))
            {
                await this.JoinRoom(roomId, userName);
                await Clients.Caller.SendAsync("JoinRoom_CreateStartButton");
            }
            else
            {
                if (!Utils.IsValidName(userName))
                {
                    await this.ShowError("Invalid UserName. A valid UserName is required to create a room.");
                }
                else
                {
                    var numRooms = Program.Instance.Rooms.Count;
                    await this.ShowError("Unable to create room. Number of total rooms: " + numRooms);
                }
            }
        }

        // To join a room
        public async Task JoinRoom(string roomId, string userName)
        {
            Room room;
            if (!Program.Instance.Rooms.TryGetValue(roomId, out room))
            {
                // Room does not exist
                await this.ShowError("Room does not exist.");
                return;
            }

            var createdPlayer = Program.Instance.JoinRoom(userName, room, Context.ConnectionId);
            if (createdPlayer != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await Clients.Group(roomId).SendAsync("JoinRoom", roomId, userName);
                await JoinRoom_ChangeState(room);
                await JoinRoom_UpdateState(room, createdPlayer);
            }
            else
            {
                await this.ShowError("Unable to create player in room. Ensure you have a valid and unique UserName.");
                return;
            }
        }

        internal async Task JoinRoom_ChangeState(Room room)
        {
            await Clients.Caller.SendAsync("JoinRoom_ChangeState", room.Id);
        }

        internal async Task JoinRoom_UpdateState(Room room, Player newPlayer)
        {
            var playerNames = new StringBuilder();
            foreach (var player in room.Players)
            {
                playerNames.Append(player.Name);
                playerNames.Append("|");
            }

            if (playerNames.Length > 0)
            {
                playerNames.Length--;
            }

            await Clients.Group(room.Id).SendAsync("JoinRoom_UpdateState", playerNames.ToString(), newPlayer.Name);
        }

        // To start a game
        public async Task StartRoom(string roomId, int betrayalReward, int maxGameLength, int maxHeistSize)
        {
            const int MINPLAYERCOUNT = 4;
            Room room;
            if (!Program.Instance.Rooms.TryGetValue(roomId, out room)
                && room.SigningUp)
            {
                await this.ShowError("Error starting room. This should never happen. Restart everything!");
                return;
            }

            if (room.Players.Count < MINPLAYERCOUNT)
            {
                await this.ShowError("Need at least " + MINPLAYERCOUNT + " players!");
                return;
            }

            await this.StartRoom_ChangeState(room, betrayalReward, maxGameLength, maxHeistSize);
        }

        internal async Task StartRoom_ChangeState(Room room, int betrayalReward, int maxGameLength, int maxHeistSize)
        {
            room.StartGame(betrayalReward, maxGameLength, maxHeistSize);

            await Clients.Group(room.Id).SendAsync("StartRoom_ChangeState", room.Id);
            await this.StartRoom_UpdateState(room);
        }

        internal async Task StartRoom_UpdateState(Room room)
        {
            foreach (var player in room.Players)
            {
                player.CurrentStatus = Player.Status.FindingHeist;
                await Clients.Client(player.ConnectionId).SendAsync("StartRoom_UpdateState", player.NetWorth, room.CurrentYear + 2018, player.Name, player.MinJailSentence, player.MaxJailSentence);
            }

            room.SigningUp = false;
            room.SpawnHeists();
            foreach (var heist in room.Heists.Values)
            {
                foreach (var player in heist.Players.Values)
                {
                    await Groups.AddToGroupAsync(player.ConnectionId, heist.Id);
                }
            }

            if (room.Heists.Count > 0)
            {
                foreach (var heist in room.Heists.Values)
                {
                    await this.HeistPrep_ChangeState(heist);
                }
            }
            else
            {
                // If there aren't enough players to start a heist, resolve it
                room.TryResolveHeists();
            }
        }

        internal async Task HeistPrep_ChangeState(Heist heist)
        {
            var random = new Random();

            var playerInfo = new StringBuilder();
            foreach (var player in heist.Players.Values)
            {
                playerInfo.Append(player.Name);
                playerInfo.Append("|");
                var fudgedNetworth = player.NetWorth * random.Next(50, 150) / 100;
                playerInfo.Append(fudgedNetworth);
                playerInfo.Append("|");
                playerInfo.Append(player.TimeSpentInJail);
                playerInfo.Append("=");
            }

            if (playerInfo.Length > 0)
            {
                playerInfo.Length = playerInfo.Length - 1;
            }

            await Clients.Group(heist.Id).SendAsync("HeistPrep_ChangeState", playerInfo.ToString(), heist.TotalReward, heist.SnitchReward);
        }

        // To make a choice in a heist
        public async Task HeistChoice(int choice)
        {
            // Player makes a choice during a heist
            // Change player's screen
        }

        public async Task HeistChoice_ChangeState(Heist heist)
        {
            // A heist gets resolved
        }
    }
}
