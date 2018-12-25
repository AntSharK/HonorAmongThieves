using HonorAmongThieves.Game;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var playerNames = "";
            foreach (var player in Program.Instance.Rooms[roomId].Players)
            {
                playerNames = player.Name + "|" + playerNames + "|";
            }

            await Clients.Group(roomId).SendAsync("JoinRoom_UpdateState", playerNames.TrimEnd('|'), newUser);
        }

        // To start a game
        public async Task StartRoom(string roomId)
        {
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
        }

        public async Task StartRoom_UpdateState(string roomId, Heist heist)
        {
            // Complex logic to resolve who is in a heist and who transitions states to be in a heist
        }

        public async Task HeistSignup(bool signsUp)
        {
            // A player opts on or out of a heist
        }

        public async Task HeistPrep_ChangeState(Heist heist)
        {
            // Change state to the heist screen
            // Add players to be in heist groups
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
