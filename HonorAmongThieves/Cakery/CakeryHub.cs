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

        // On connected
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("FreshConnection");
            await base.OnConnectedAsync();
        }

        // Display an error message
        internal async Task ShowError(string errorMessage)
        {
            await Clients.Caller.SendAsync("ShowError", errorMessage);
        }

        // Room creation
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

        // Joining Room
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
            else if (!room.SettingUp
                && room.Players.ContainsKey(userName))
            {
                // Take over the existing session
                await this.ResumeSession(roomId, userName);
                return;
            }
            else
            {
                await this.ShowError("Unable to create player in room. Ensure you have a valid and unique UserName.");
                return;
            }
        }

        internal async Task JoinRoom_UpdateView(CakeryRoom room, CakeryPlayer newPlayer)
        {
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

        public async Task StartRoom(string roomId, int gameLength, int startingCash)
        {
#if DEBUG
            const int MINPLAYERCOUNT = 1;
#else
            const int MINPLAYERCOUNT = 2;
#endif
            CakeryRoom room;
            if (!this.lobby.Rooms.TryGetValue(roomId, out room)
                && room.SettingUp)
            {
                await this.ShowError("This room has timed out! Please refresh the page.");
                return;
            }

            if (room.Players.Count < MINPLAYERCOUNT)
            {
                await this.ShowError("Need at least " + MINPLAYERCOUNT + " players!");
                return;
            }

            room.StartGame(gameLength, startingCash);
            await this.UpdateRoom_NextRound(room);
        }

        // Updates everyone in the room for the next round
        public async Task UpdateRoom_NextRound(CakeryRoom room)
        {
            // All calculations for costs should be done by now
            foreach (var player in room.Players.Values)
            {
                await Clients.Client(player.ConnectionId).SendAsync("UpdateProductionState",
                    room.CurrentPrices,
                    room.CurrentMarket,
                    player.CurrentResources,
                    player.CurrentUpgrades,
                    player.CurrentBakedGoods);
            }
        }

        // Resuming session
        public async Task ResumeSession(string roomId, string userName)
        {
            CakeryRoom room;
            if (!this.lobby.Rooms.TryGetValue(roomId, out room))
            {
                await this.ShowError("Cannot find Room ID.");
                await Clients.Caller.SendAsync("ClearState");
                return;
            }

            CakeryPlayer player;
            if (!room.Players.TryGetValue(userName, out player))
            {
                await this.ShowError("Cannot find player in room.");
                await Clients.Caller.SendAsync("ClearState");
                return;
            }

            player.ConnectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            switch (player.CurrentStatus)
            {
                case CakeryPlayer.Status.WaitingForGameStart:
                    await this.JoinRoom_UpdateView(room, player);
                    break;
                case CakeryPlayer.Status.Producing:
                    await Clients.Caller.SendAsync("UpdateProductionState",
                            room.CurrentPrices,
                            room.CurrentMarket,
                            player.CurrentResources,
                            player.CurrentUpgrades,
                            player.CurrentBakedGoods);
                    break;
                case CakeryPlayer.Status.SettingUpShop:
                    await Clients.Caller.SendAsync("SetUpShop",
                            player.CurrentResources,
                            player.CurrentUpgrades,
                            player.CurrentBakedGoods);
                    break;
                case CakeryPlayer.Status.MarketReport:
                    await Clients.Caller.SendAsync("MarketReport",
                            room.CurrentPrices,
                            room.CurrentMarket,
                            player.CurrentBakedGoods);
                    break;
            }
        }

        // Buying ingredients for a player
        public async Task BuyIngredients(string roomId, string playerName, int butterBought, int flourBought, int sugarBought)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (player.MakePurchase(butterBought, flourBought, sugarBought))
            {
                await Clients.Caller.SendAsync("UpdateProductionState",
                        room.CurrentPrices,
                        room.CurrentMarket,
                        player.CurrentResources,
                        player.CurrentUpgrades,
                        player.CurrentBakedGoods);
            }
            else
            {
                await ShowError("Error purchasing ingredients.");
            }
        }
    }
}
