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
                await JoinRoom_ChangeState(room, createdPlayer);
                await JoinRoom_UpdateState(room, createdPlayer);
            }
            else
            {
                await this.ShowError("Unable to create player in room. Ensure you have a valid and unique UserName.");
                return;
            }
        }

        internal async Task JoinRoom_ChangeState(Room room, Player player)
        {
            await Clients.Caller.SendAsync("JoinRoom_ChangeState", room.Id, player.Name);
        }

        internal async Task JoinRoom_UpdateState(Room room, Player newPlayer)
        {
            var playerNames = new StringBuilder();
            foreach (var player in room.Players.Values)
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
                await this.ShowError("This room has timed out! Please refresh the page.");
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
            room.UpdatedTime = DateTime.UtcNow; // Only update the room when the players click something

            await Clients.Group(room.Id).SendAsync("StartRoom_ChangeState", room.Id);
            await this.StartRoom_UpdateState(room);
        }

        internal async Task StartRoom_UpdateState(Room room)
        {
            foreach (var player in room.Players.Values)
            {
                player.CurrentStatus = Player.Status.FindingHeist;
                await Clients.Client(player.ConnectionId).SendAsync("StartRoom_UpdateState", player.NetWorth, room.CurrentYear + 2018, player.Name, player.MinJailSentence, player.MaxJailSentence);
            }

            // TODO: Stupid testing stuff
            var k = 0;
            foreach (var player in room.Players.Values)
            {
                switch(k)
                {
                    case 0:
                        player.CurrentStatus = Player.Status.Dead;
                        break;
                    case 1:
                        player.CurrentStatus = Player.Status.InJail;
                        player.YearsLeftInJail = 3;
                        break;
                    case 2:
                    case 3:
                        break;
                }

                k++;
            }
            // End stupid testing stuff

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

            // Update the message for each player who can't act
            foreach (var player in room.Players.Values)
            {
                switch (player.CurrentStatus)
                {
                    case Player.Status.FindingHeist:
                        await Clients.Client(player.ConnectionId).SendAsync("UpdateHeistStatus", "FINDING HEIST...",
                            "Your contacts don't seem to be responding. If there is any crime going on, you're not being invited.");
                        break;
                    case Player.Status.Dead:
                        await Clients.Client(player.ConnectionId).SendAsync("UpdateHeistStatus", "DEAD",
                            "You're dead. You can't do anything.");
                        break;
                    case Player.Status.InJail:
                        await Clients.Client(player.ConnectionId).SendAsync("UpdateHeistStatus", "IN JAIL",
                            "You're IN JAIL. Years left: " + player.YearsLeftInJail);
                        break;
                }
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
    }
}
