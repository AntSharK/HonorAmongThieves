using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Cakery.GameLogic
{
    public class CakeryRoom : Room<CakeryPlayer, CakeryHub>
    {
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

        public Prices CurrentPrices { get; } = new Prices();
        public Market CurrentMarket { get; } = new Market();

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

            this.SettingUp = false;
        }
    }
}
