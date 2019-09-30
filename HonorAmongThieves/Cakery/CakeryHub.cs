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

        public async Task StartRoom(string roomId, int gameLength, int startingCash, int annualallowance)
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

            room.StartGame(gameLength, startingCash, annualallowance);
            await this.UpdateRoom_NextRound(room);
        }

        // Updates everyone in the room for the next round
        public async Task UpdateRoom_NextRound(CakeryRoom room)
        {
            // All calculations for costs should be done by now
            foreach (var player in room.Players.Values)
            {
                await this.UpdateProductionState(room, player, false /*Don't update the caller*/);
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
                    await UpdateProductionState(room, player);
                    break;
                case CakeryPlayer.Status.SettingUpShop:
                    var readyPlayerNames = from readyPlayer
                                           in room.Players.Values
                                           where readyPlayer.CurrentStatus != CakeryPlayer.Status.Producing
                                           select readyPlayer.Name;

                    var notReadyPlayerNames = from notReadyPlayer
                                           in room.Players.Values
                                              where notReadyPlayer.CurrentStatus == CakeryPlayer.Status.Producing
                                              select notReadyPlayer.Name;

                    await Clients.Caller.SendAsync("SetUpShop",
                            room.CurrentPrices,
                            room.CurrentMarket,
                            player.CurrentResources,
                            player.CurrentUpgrades,
                            player.CurrentBakedGoods,
                            readyPlayerNames,
                            notReadyPlayerNames);
                    break;
                case CakeryPlayer.Status.MarketReport:
                    var yearToDisplay = room.CurrentMarket.CurrentYear - 1;
                    var marketReport = room.MarketReports[yearToDisplay];
                    await room.DisplayMarketReport(player, marketReport, room.CurrentPrices);
                    break;
                case CakeryPlayer.Status.CleaningUp:
                    await Clients.Caller.SendAsync("EndGame", room.FinalTotalSalesData, room.FinalYearlySalesData[player]);
                    break;
            }
        }

        private async Task UpdateProductionState(CakeryRoom room, CakeryPlayer player, bool updateCaller = true)
        {
            var collatedUpgradeList = from newUpgrades
                                        in player.JustPurchasedUpgrades.Values
                                      select new KeyValuePair<string, int>(newUpgrades.Name, newUpgrades.AmountOwned);

            var collatedDictionary = collatedUpgradeList.ToDictionary(x => x.Key.ToLower(), x => x.Value);

            if (updateCaller)
            {
                await Clients.Caller.SendAsync("UpdateProductionState",
                        room.CurrentPrices,
                        room.CurrentMarket,
                        player.CurrentResources,
                        player.CurrentUpgrades.Values,
                        collatedDictionary,
                        player.CurrentBakedGoods);
            }
            else
            {
                await Clients.Client(player.ConnectionId).SendAsync("UpdateProductionState",
                        room.CurrentPrices,
                        room.CurrentMarket,
                        player.CurrentResources,
                        player.CurrentUpgrades.Values,
                        collatedDictionary,
                        player.CurrentBakedGoods);
            }
        }

        // Buying ingredients for a player
        public async Task BuyIngredients(string roomId, string playerName, double butterBought, double flourBought, double sugarBought)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (player.MakePurchase(butterBought, flourBought, sugarBought))
            {
                await UpdateProductionState(room, player);
            }
            else
            {
                await ShowError("Error purchasing ingredients.");
            }
        }

        // Baking goods for a player
        public async Task BakeGoods(string roomId, string playerName, int cookiesBaked, int croissantsBaked, int cakesBaked)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (player.BakeGoods(cookiesBaked, croissantsBaked, cakesBaked))
            {
                await UpdateProductionState(room, player);
            }
            else
            {
                await ShowError("Error baking goods.");
            }
        }

        // Purchasing upgrades for a player
        public async Task BuyUpgrades(string roomId, string playerName, IDictionary<string, int> upgradesBought)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (player.BuyUpgrades(upgradesBought))
            {
                await UpdateProductionState(room, player);
            }
            else
            {
                await ShowError("Error buying upgrades.");
            }
        }

        // Using upgrades for player
        public async Task UseUpgrade(string roomId, string playerName, string upgradeName, int amountToUse)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (player.UseUpgrade(upgradeName, amountToUse))
            {
                await UpdateProductionState(room, player);
            }
            else
            {
                await ShowError("Error using upgrades.");
            }
        }

        // Player decides to set up shop
        public async Task SetUpShop(string roomId, string playerName)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            await room.EndPlayerTurn(player);
        }

        // Player ends market report
        public async Task EndMarketReport(string roomId, string playerName)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            if (room.CurrentMarket.CurrentYear < room.CurrentMarket.MaxYears)
            {
                player.CurrentStatus = CakeryPlayer.Status.Producing;
                await UpdateProductionState(room, player);
            }
            else
            {
                player.CurrentStatus = CakeryPlayer.Status.CleaningUp;
                await Clients.Caller.SendAsync("EndGame", room.FinalTotalSalesData, room.FinalYearlySalesData[player]);
            }
        }
    }
}
