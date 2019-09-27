using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Cakery.GameLogic
{
    public class CakeryRoom : Room<CakeryPlayer, CakeryHub>
    {
        private static Random random = new Random();

        // Global prices
        public class Prices
        {
            // Buy-price of raw materials
            public double Flour = 100f;
            public double Butter = 100f;
            public double Sugar = 100f;

            // Sell-price of baked goods
            public double Cookies = 250f; // Costs 130 to make
            public double Croissants = 450f; // Costs 250 to make
            public double Cakes = 2750f; // Costs 2250 to make

            // TODO (Upgrades) Buy-price of upgrades
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
            public Dictionary<Player, double> PlayerProfits = new Dictionary<Player, double>();
            public (long cookiesSold, long croissantsSold, long cakesSold) TotalSales;
            public double CashInPreviousRound;
        }

        public Prices CurrentPrices { get; } = new Prices();
        public Market CurrentMarket { get; } = new Market();
        public MarketReport[] MarketReports;
        public double CashInGame = 0;

        public IEnumerable<(string, double, double, double, double)> FinalTotalSalesData;
        public Dictionary<Player, (double cookiesSold, double croissantsSold, double cakesSold, double totalProfit)[]> FinalYearlySalesData = new Dictionary<Player, (double cookiesSold, double croissantsSold, double cakesSold, double totalProfit)[]>();

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
            this.CashInGame = startingCash * this.Players.Count;
            this.SettingUp = false;
        }

        public async Task EndPlayerTurn(CakeryPlayer readyPlayer)
        {
            readyPlayer.CurrentStatus = CakeryPlayer.Status.SettingUpShop;
            
            var readyPlayerNames = from player 
                                   in this.Players.Values
                                   where player.CurrentStatus == CakeryPlayer.Status.SettingUpShop
                                   select player.Name;

            var notReadyPlayerNames = from player
                                   in this.Players.Values
                                   where player.CurrentStatus != CakeryPlayer.Status.SettingUpShop
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
            this.UpdatedTime = DateTime.UtcNow;
            var marketReport = new MarketReport();

            // Get total sales and compute the prices of baked goods
            marketReport.TotalSales = (0, 0, 0);
            foreach (var player in this.Players.Values)
            {
                marketReport.TotalSales = (marketReport.TotalSales.cookiesSold + player.CurrentBakedGoods.Cookies,
                    marketReport.TotalSales.croissantsSold + player.CurrentBakedGoods.Croissants,
                    marketReport.TotalSales.cakesSold + player.CurrentBakedGoods.Cakes);
            }

            ComputeMarketPrices(this.CurrentPrices, this.CashInGame, marketReport);

            // Sell all goods baked
            marketReport.CashInPreviousRound = this.CashInGame;
            foreach (var player in this.Players.Values)
            {
                player.SellGoods(marketReport);
                this.CashInGame = this.CashInGame + player.CurrentResources.Money;
            }

            // Then, finalize upgrades
            foreach (var player in this.Players.Values)
            {
                // TODO
                // player.FinalizeUpgrades();
            }

            // Finally, broadcast this year's report to each player
            this.MarketReports[this.CurrentMarket.CurrentYear] = marketReport;
            this.CurrentMarket.CurrentYear++;

            if (this.CurrentMarket.CurrentYear >= this.CurrentMarket.MaxYears)
            {
                this.GenerateEndGameReport();
            }

            foreach (var player in this.Players.Values)
            {
                player.CurrentStatus = CakeryPlayer.Status.MarketReport;
                await this.DisplayMarketReport(player, marketReport, this.CurrentPrices);
            }
        }

        public void GenerateEndGameReport()
        {
            var year = 0;
            var AllCookiesSold = new Dictionary<Player, double>();
            var AllCroissantsSold = new Dictionary<Player, double>();
            var AllCakesSold = new Dictionary<Player, double>();
            var AllProfits = new Dictionary<Player, double>();

            foreach (var player in this.Players.Values)
            {
                this.FinalYearlySalesData[player] = new (double, double, double, double)[this.CurrentMarket.MaxYears];
                AllCookiesSold[player] = 0;
                AllCroissantsSold[player] = 0;
                AllCakesSold[player] = 0;
                AllProfits[player] = 0;
            }

            foreach (var report in this.MarketReports)
            {
                foreach (var goodsSold in report.PlayerSalesData)
                {
                    AllCookiesSold[goodsSold.Key] += goodsSold.Value.cookiesSold;
                    AllCroissantsSold[goodsSold.Key] += goodsSold.Value.croissantsSold;
                    AllCakesSold[goodsSold.Key] += goodsSold.Value.cakesSold;
                }

                foreach (var profits in report.PlayerProfits)
                {
                    AllProfits[profits.Key] += profits.Value;

                    this.FinalYearlySalesData[profits.Key][year] =
                        (report.PlayerSalesData[profits.Key].cookiesSold, report.PlayerSalesData[profits.Key].croissantsSold,
                        report.PlayerSalesData[profits.Key].cakesSold, profits.Value);
                }

                year++;
            }

            var finalTotalSalesData = new List<(string, double cookiesSold, double croissantsSold, double cakesSold, double totalProfit)>();
            foreach (var player in this.Players.Values)
            {
                finalTotalSalesData.Add((player.Name, AllCookiesSold[player], AllCroissantsSold[player], AllCakesSold[player], AllProfits[player]));
            }

            FinalTotalSalesData = from entry in finalTotalSalesData
                        orderby entry.totalProfit descending
                        select entry;
        }

        public async Task DisplayMarketReport(CakeryPlayer player, MarketReport marketReport, Prices currentPrices)
        {
            var newsReport = TextGenerator.GetExactMarketReport(marketReport, currentPrices);
            var playerSales = marketReport.PlayerSalesData[player];
            var playerProfit = marketReport.PlayerProfits[player];
            var goodPrices = marketReport.Prices;
            await this.hubContext.Clients.Client(player.ConnectionId).SendAsync("ShowMarketReport",
                newsReport, playerSales, goodPrices,
                playerProfit, this.CurrentMarket);
        }

        public static (double, double, double) ComputeExpectedSales(double cashInGame)
        {
            double expectedCookies = cashInGame / 400;
            double expectedCroissants = cashInGame / 800;
            double expectedCakes = cashInGame / 7000;

            return (expectedCookies, expectedCroissants, expectedCakes);
        }

        private static void ComputeMarketPrices(Prices currentPrices, double cashInGame, MarketReport marketReport)
        {
            // First, re-calculate baked goods cost
            // Cookie base cost can go up or down by up to 10%
            currentPrices.Cookies = currentPrices.Cookies * random.Next(900, 1100) * 0.001f;

            // Croissant base cost goes up or down by up to 5%
            currentPrices.Croissants = currentPrices.Croissants * random.Next(950, 1050) * 0.001f;

            // Cake base cost goes up or down by up to 20%
            currentPrices.Cakes = currentPrices.Cakes * random.Next(800, 1200) * 0.001f;

            // Then, adjust current cost based on number of goods in the market
            (var expectedCookies, var expectedCroissants, var expectedCakes) = ComputeExpectedSales(cashInGame);

            // Cookies - 200% at 0, 120% at quarter, 80% at expected quantity, 40% at double expected quantity
            marketReport.Prices.cookiePrice = currentPrices.Cookies *
                ComputeMultiplier(expectedCookies, marketReport.TotalSales.cookiesSold, 2.0, 1.2, 0.8, 0.4);

            // Croissants - 130% at 0, 100% at quarter, 90% at expected quantity, 75% at double expected quantity
            marketReport.Prices.croissantPrice = currentPrices.Croissants *
                ComputeMultiplier(expectedCroissants, marketReport.TotalSales.croissantsSold, 1.4, 1.0, 0.9, 0.75);

            // Cakes - 400% at 0, 200% at quarter, 100% at expected quantity, 50% at double expected quantity
            marketReport.Prices.cakePrice = currentPrices.Cakes *
                ComputeMultiplier(expectedCakes, marketReport.TotalSales.cakesSold, 4.0, 2.0, 1.0, 0.5);
        }

        private static double ComputeMultiplier(double expectedSales, double actualSales,
            double zeroPoint, double quarterPoint, double expectedPoint, double doublePoint)
        {
            var percentageOfExpectedSales = actualSales / (expectedSales != 0 ? expectedSales : 0.1);
            if (percentageOfExpectedSales <= 0.25)
            {
                return zeroPoint + (percentageOfExpectedSales - 0) * (quarterPoint - zeroPoint) / (0.25 - 0);
            }
            else if (percentageOfExpectedSales < 1.0)
            {
                return quarterPoint + (percentageOfExpectedSales - 0.25) * (expectedPoint - quarterPoint) / (1.0 - 0.25);
            }
            else if (percentageOfExpectedSales < 2.0)
            {
                return expectedPoint + (percentageOfExpectedSales - 1) * (doublePoint - expectedPoint) / (2.0 - 1.0);
            }

            return doublePoint;
        }
    }
}
