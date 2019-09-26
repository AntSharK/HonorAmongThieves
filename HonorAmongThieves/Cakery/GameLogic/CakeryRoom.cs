﻿using Microsoft.AspNetCore.SignalR;
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
            public double Cookies = 200f; // Costs 130 to make
            public double Croissants = 400f; // Costs 250 to make
            public double Cakes = 2500f; // Costs 2250 to make

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
        public double CashInGame = 0;

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
            var lastRoundCashInGame = this.CashInGame;
            this.CashInGame = 0;
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
            var newsReport = TextGenerator.GetNewsReport(lastRoundCashInGame, marketReport);

            this.CurrentMarket.CurrentYear++;
            if (this.CurrentMarket.CurrentYear > this.CurrentMarket.MaxYears)
            {
                // TODO: End the game
            }
            else
            {
                foreach (var player in this.Players.Values)
                {
                    player.CurrentStatus = CakeryPlayer.Status.Producing;
                    var playerSales = marketReport.PlayerSalesData[player];
                    var goodPrices = marketReport.Prices;
                    await this.hubContext.Clients.Client(player.ConnectionId).SendAsync("ShowMarketReport",
                        newsReport, playerSales, goodPrices);
                }
            }
        }

        public static (double, double, double) ComputeExpectedSales(double cashInGame)
        {
            double expectedCookies = cashInGame / 150;
            double expectedCroissants = cashInGame / 300;
            double expectedCakes = cashInGame / 3000;

            return (expectedCookies, expectedCroissants, expectedCookies);
        }

        private static void ComputeMarketPrices(Prices currentPrices, double cashInGame, MarketReport marketReport)
        {
            // First, re-calculate baked goods cost
            // Cookie base cost can go up or down by up to 10%
            currentPrices.Cookies = currentPrices.Cookies * random.Next(900, 1100) * 0.1f;

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
            var percentageOfExpectedSales = actualSales / (expectedSales != 0 ? expectedSales : 0.001);
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
