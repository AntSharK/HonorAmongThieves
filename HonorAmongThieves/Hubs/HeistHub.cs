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
            if (Program.Instance.JoinRoom(userName, roomId, Context.ConnectionId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await Clients.Group(roomId).SendAsync("JoinRoom", roomId, userName);

                var playerNames = "";
                foreach (var player in Program.Instance.Rooms[roomId].Players)
                {
                    playerNames = player.Name + "|" + playerNames + "|";
                }

                await Clients.Group(roomId).SendAsync("UpdateRoomList", playerNames.TrimEnd('|'));
            }
        }

        public async Task CreateRoom(string userName)
        {
            if (userName.Length == 0)
            {
                return;
            }

            foreach (char c in userName.ToCharArray())
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return;
                }
            }

            var roomId = Program.Instance.CreateRoom();
            if (!string.IsNullOrEmpty(roomId))
            {
                await this.JoinRoom(roomId, userName);
            }

            await Clients.Group(roomId).SendAsync("CreateStartButton", userName);
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
