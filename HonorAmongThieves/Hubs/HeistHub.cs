using HonorAmongThieves.Game;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonorAmongThieves.Hubs
{
    public class HeistHub : Hub
    {
        // To create a room
        public async Task CreateRoom(string userName)
        {
            var roomId = Program.Instance.CreateRoom(this, userName);
            if (!string.IsNullOrEmpty(roomId))
            {
                await this.JoinRoom(roomId, userName);
                await Clients.Caller.SendAsync("JoinRoom_CreateStartButton");
            }
        }

        // To join a room
        public async Task JoinRoom(string roomId, string userName)
        {
            var createdPlayer = Program.Instance.JoinRoom(userName, roomId, Context.ConnectionId);
            if (createdPlayer != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await Clients.Group(roomId).SendAsync("JoinRoom", roomId, userName);
                await JoinRoom_ChangeState(roomId);
                await JoinRoom_UpdateState(roomId, userName);
            }
        }

        public async Task JoinRoom_ChangeState(string roomId)
        {
            await Clients.Caller.SendAsync("JoinRoom_ChangeState", roomId);
        }

        public async Task JoinRoom_UpdateState(string roomId, string newUser)
        {
            var playerNames = new StringBuilder();
            foreach (var player in Program.Instance.Rooms[roomId].Players)
            {
                playerNames.Append(player.Name);
                playerNames.Append("|");
            }

            if (playerNames.Length > 0)
            {
                playerNames.Length--;
            }

            await Clients.Group(roomId).SendAsync("JoinRoom_UpdateState", playerNames.ToString(), newUser);
        }

        // To start a game
        public async Task StartRoom(string roomId, int betrayalReward, int maxGameLength, int maxHeistSize)
        {
            // TODO: Put custom parameters into room

            const int MINPLAYERCOUNT = 4;

            if (Program.Instance.Rooms.ContainsKey(roomId)
                && Program.Instance.Rooms[roomId].SigningUp
                && Program.Instance.Rooms[roomId].Players.Count >= MINPLAYERCOUNT)
            {
                Program.Instance.Rooms[roomId].SigningUp = false;
                await this.StartRoom_ChangeState(roomId);
            }
        }

        public async Task StartRoom_ChangeState(string roomId)
        {
            await Clients.Group(roomId).SendAsync("StartRoom_ChangeState", roomId);
            await this.StartRoom_UpdateState(roomId);
        }

        public async Task StartRoom_UpdateState(string roomId)
        {
            var room = Program.Instance.Rooms[roomId];

            foreach (var player in room.Players)
            {
                player.CurrentStatus = Player.Status.FindingHeist;
                await Clients.Client(player.ConnectionId).SendAsync("StartRoom_UpdateState", player.NetWorth, room.Years + 2018, player.Name);
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

        public async Task HeistPrep_ChangeState(Heist heist)
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

            await Clients.Group(heist.Id).SendAsync("HeistPrep_ChangeState", playerInfo.ToString());
        }


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
