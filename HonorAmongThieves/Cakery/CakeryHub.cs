using HonorAmongThieves.Cakery.GameLogic;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonorAmongThieves.Cakery
{
    public class CakeryHub : Hub
    {
        private readonly CakeryGame lobby;

        public CakeryHub(CakeryGame lobby)
        {
            this.lobby = lobby;
        }

        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("FreshConnection");
            await base.OnConnectedAsync();
        }

        internal async Task ShowError(string errorMessage)
        {
            await Clients.Caller.SendAsync("ShowError", errorMessage);
        }

        public async Task CreateRoom(string userName)
        {
            var roomId = this.lobby.CreateRoom(userName);
            if (!string.IsNullOrEmpty(roomId))
            {
                await this.JoinRoom(roomId, userName);
            }
            else
            {
                if (!Utils.IsValidName(userName))
                {
                    await this.ShowError("Invalid UserName. A valid UserName is required to create a room.");
                }
                else
                {
                    var numRooms = this.lobby.Rooms.Count;
                    await this.ShowError("Unable to create room. Number of total rooms: " + numRooms);
                }
            }
        }

        public async Task JoinRoom(string roomId, string userName)
        {
            CakeryRoom room;
            if (!this.lobby.Rooms.TryGetValue(roomId, out room))
            {
                // Room does not exist
                await this.ShowError("Room does not exist.");
                return;
            }

            var createdPlayer = this.lobby.JoinRoom(userName, room, Context.ConnectionId);
            if (createdPlayer != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await this.JoinRoom_UpdateView(room, createdPlayer);
            }
            else
            {
                await this.ShowError("Unable to create player in room. Ensure you have a valid and unique UserName.");
                return;
            }
        }

        internal async Task JoinRoom_UpdateView(CakeryRoom room, CakeryPlayer newPlayer)
        {
            // TODO: LOTS OF STUFF
            await Clients.Group(room.Id).SendAsync("JoinRoom", room.Id, newPlayer.Name);
            await Clients.Caller.SendAsync("JoinRoom_ChangeState", room.Id, newPlayer.Name);

            if (room.OwnerName == newPlayer.Name)
            {
                await Clients.Caller.SendAsync("JoinRoom_CreateStartButton");
            }

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
    }
}
