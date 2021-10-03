using HonorAmongThieves.Cakery.GameLogic;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Cakery
{
    public class CakeryHub : GameHub<CakeryHub, CakeryRoom, CakeryPlayer>
    {
        public CakeryHub(CakeryGame lobby)
        {
            this.lobby = lobby;
        }

        /// <inheritdoc />
        public override async Task ResumePlayerSession(CakeryPlayer player)
        {
            switch (player.CurrentStatus)
            {
                case CakeryPlayer.Status.WaitingForGameStart:
                    await this.JoinRoom_UpdateView(player.Room, player);
                    break;
                case CakeryPlayer.Status.Producing:
                    await UpdateProductionState(player.Room, player);
                    break;
                case CakeryPlayer.Status.SettingUpShop:
                    var readyPlayerNames = from readyPlayer
                                           in player.Room.Players.Values
                                           where readyPlayer.CurrentStatus != CakeryPlayer.Status.Producing
                                           select readyPlayer.Name;

                    var notReadyPlayerNames = from notReadyPlayer
                                           in player.Room.Players.Values
                                              where notReadyPlayer.CurrentStatus == CakeryPlayer.Status.Producing
                                              select notReadyPlayer.Name;

                    await Clients.Caller.SendAsync("SetUpShop",
                            player.Room.CurrentPrices,
                            player.Room.CurrentMarket,
                            player.CurrentResources,
                            player.CurrentUpgrades,
                            player.CurrentBakedGoods,
                            readyPlayerNames,
                            notReadyPlayerNames);
                    break;
                case CakeryPlayer.Status.MarketReport:
                    var yearToDisplay = player.Room.CurrentMarket.CurrentYear - 1;
                    var marketReport = player.Room.MarketReports[yearToDisplay];
                    await player.Room.DisplayMarketReport(player, marketReport, player.Room.CurrentPrices);
                    break;
                case CakeryPlayer.Status.CleaningUp:
                    await Clients.Caller.SendAsync("EndGame", player.Room.FinalTotalSalesData, player.Room.FinalYearlySalesData[player]);
                    break;
            }
        }

        public async Task StartRoom(string roomId, int gameLength, int startingCash, int annualAllowance, int upgradeAllowance)
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

            room.StartGame(gameLength, startingCash, annualAllowance, upgradeAllowance);
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

            await room.EndPlayerTurn(player, true /*End Turn*/);
        }

        // Player decides to set up shop
        public async Task BackToBakery(string roomId, string playerName)
        {
            var room = this.lobby.Rooms[roomId];
            var player = room.Players[playerName];

            await room.EndPlayerTurn(player, false /*Revert turn ending*/);
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
