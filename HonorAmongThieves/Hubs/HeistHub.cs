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

        public async Task CreateRoom(string userName)
        {
            var roomId = Program.Instance.CreateRoom(this, userName);
            if (!string.IsNullOrEmpty(roomId))
            {
                await this.JoinRoom(roomId, userName);
                await Clients.Caller.SendAsync("JoinRoom_CreateStartButton");
            }
        }

        public async Task StartRoom(string roomId)
        {
            const int MINPLAYERCOUNT = 4;

            if (Program.Instance.Rooms.ContainsKey(roomId)
                && Program.Instance.Rooms[roomId].SigningUp
                && Program.Instance.Rooms[roomId].Players.Count >= MINPLAYERCOUNT)
            {
                Program.Instance.Rooms[roomId].SigningUp = false;
                await Clients.Group(roomId).SendAsync("StartGame");
            }
        }
    }
}
