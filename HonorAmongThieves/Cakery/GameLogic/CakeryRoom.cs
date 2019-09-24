using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Cakery.GameLogic
{
    public class CakeryRoom : Room<CakeryPlayer, CakeryHub>
    {
        // Global prices
        public class Prices
        {
            // Buy-price of raw materials
            public double Flour = 100f;
            public double Butter = 100f;
            public double Sugar = 100f;

            // Sell-price of baked goods
            public double Cookies = 300f;
            public double Croissants = 1000f;
            public double Cakes = 20000f;

            // Buy-price of upgrades
        }

        public class Market
        {
            public long Cookies = 0;
            public long Croissants = 0;
            public long Cakes = 0;
            public int CurrentYear = 0;
            public int MaxYears = 10;
        }

        public class MarketReport
        {
            public (double cookiePrice, double croissantPrice, double cakePrice) Prices;
            public Dictionary<Player, (long cookiesSold, long croissantsSold, long cakesSold)> PlayerSalesData = new Dictionary<Player, (long cookiesSold, long croissantsSold, long cakesSold)>();
            public Dictionary<Player, long> PlayerProfits = new Dictionary<Player, long>();
            public (long cookiesSold, long croissantsSold, long cakesSold) TotalSales;
        }

        public Prices CurrentPrices { get; } = new Prices();
        public Market CurrentMarket { get; } = new Market();
        public MarketReport[] MarketReports;
        public int LifetimeGlobalSales = 0; // The sum of all the money given to all the players - the more money in the system, the higher the price of goods!

        public override void Destroy()
        {
            foreach (var player in this.Players.Values)
            {
                player.CurrentStatus = CakeryPlayer.Status.CleaningUp;
            }
        }

        public CakeryRoom(string id, IHubContext<CakeryHub> hubContext)
            : base(id, hubContext) { }

        protected override CakeryPlayer InstantiatePlayer(string playerName)
        {
            return new CakeryPlayer(playerName, this);
        }

        public void StartGame(int gameLength, int startingCash)
        {
            this.CurrentMarket.MaxYears = gameLength;
            foreach (var player in this.Players.Values)
            {
                player.CurrentStatus = CakeryPlayer.Status.Producing;
                player.CurrentResources.Money = startingCash;
            }

            this.MarketReports = new MarketReport[gameLength];
            this.LifetimeGlobalSales = startingCash * this.Players.Count;
            this.SettingUp = false;
        }

        public async Task EndPlayerTurn(CakeryPlayer readyPlayer)
        {
            readyPlayer.CurrentStatus = CakeryPlayer.Status.SettingUpShop;
            
            var readyPlayerNames = from player 
                                   in this.Players.Values
                                   where player.CurrentStatus != CakeryPlayer.Status.Producing
                                   select player.Name;

            var notReadyPlayerNames = from player
                                   in this.Players.Values
                                   where player.CurrentStatus == CakeryPlayer.Status.Producing
                                   select player.Name;

            var readyPlayers = this.Players.Values.Where(p => p.CurrentStatus != CakeryPlayer.Status.Producing);
            foreach (var player in readyPlayers)
            {
                await this.hubContext.Clients.Client(player.ConnectionId).SendAsync("UpdatePlayerList",
                    readyPlayerNames,
                    notReadyPlayerNames);
            }

            // Advance to the next state if everyone has ended their turn
            if (notReadyPlayerNames.Count() <= 0)
            {
                await this.EndRound();
            }
        }

        public async Task EndRound()
        {
            var marketReport = new MarketReport();
            marketReport.TotalSales = (0, 0, 0);

            foreach (var player in this.Players.Values)
            {
                marketReport.PlayerSalesData[player] = (player.CurrentBakedGoods.Cookies, player.CurrentBakedGoods.Croissants, player.CurrentBakedGoods.Cakes);
                marketReport.TotalSales = (marketReport.TotalSales.cookiesSold + player.CurrentBakedGoods.Cookies,
                    marketReport.TotalSales.croissantsSold + player.CurrentBakedGoods.Croissants,
                    marketReport.TotalSales.cakesSold + player.CurrentBakedGoods.Cakes);
            }

            // First, re-calculate baked goods cost
            // There is a base cost difference
            // Then, based on the current market, 
            // TODO

            // Then, sell all goods baked
            foreach (var player in this.Players.Values)
            {
                // TODO
                player.SellGoods(marketReport);
            }

            // Then, finalize upgrades
            foreach (var player in this.Players.Values)
            {
                // TODO
                player.FinalizeUpgrades();
            }

            // Re-calculate raw material cost

            // Finally, broadcast this year's report
            // TODO
        }
    }
}
