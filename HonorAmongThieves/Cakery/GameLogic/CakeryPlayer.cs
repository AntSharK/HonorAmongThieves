using HonorAmongThieves.Cakery.GameLogic.Upgrades;
using System;
using System.Collections.Generic;
using System.Text;

namespace HonorAmongThieves.Cakery.GameLogic
{
    public class CakeryPlayer : Player
    {
        public enum Status
        {
            WaitingForGameStart,
            Producing,
            SettingUpShop,
            MarketReport,
            CleaningUp
        }

        public class Resources
        {
            public double Butter = 0;
            public double Flour = 0;
            public double Sugar = 0;
            public double Money = 0;
        }

        public class BakedGoods
        {
            public long Cookies = 0;
            public (double butter, double flour, double sugar, double money) CookieCost =
                (100d, 100d, 100d, 100d);
            public long Croissants = 0;
            public (double butter, double flour, double sugar, double money) CroissantCost =
                (500d, 500d, 500d, 100d);
            public long Cakes = 0;
            public (double butter, double flour, double sugar, double money) CakeCost =
                (2500d, 2500d, 2500d, 1500d);
        }

        public CakeryRoom Room { get; set; }

        public Status CurrentStatus { get; set; } = Status.WaitingForGameStart;

        // Does not include upgrades that are just purchased this round - should include an empty list of all upgrades
        public Dictionary<string, Upgrade> CurrentUpgrades;

        // Does not include upgrades that are just purchased this round - should include the list of every single upgrade
        public Dictionary<string, Upgrade> JustPurchasedUpgrades;

        public Resources CurrentResources { get; set; } = new Resources();
        public BakedGoods CurrentBakedGoods { get; set; } = new BakedGoods();
        public long TotalSales { get; set; } = 0;

        public CakeryPlayer(string playerName, CakeryRoom room)
            : base(playerName)
        {
            this.Room = room;
            this.CurrentUpgrades = Upgrade.Initialize(this);
            this.JustPurchasedUpgrades = Upgrade.Initialize(this);
        }

        internal bool MakePurchase(double butterBought, double flourBought, double sugarBought)
        {
            var totalCost = butterBought * Room.CurrentPrices.Butter
                + flourBought * Room.CurrentPrices.Flour
                + sugarBought * Room.CurrentPrices.Sugar;

            if (totalCost <= this.CurrentResources.Money)
            {
                this.CurrentResources.Money = this.CurrentResources.Money - totalCost;
                this.CurrentResources.Butter = this.CurrentResources.Butter + butterBought * 1000;
                this.CurrentResources.Flour = this.CurrentResources.Flour + flourBought * 1000;
                this.CurrentResources.Sugar = this.CurrentResources.Sugar + sugarBought * 1000;
                return true;
            }

            return false;
        }

        internal bool BakeGoods(int cookiesBaked, int croissantsBaked, int cakesBaked)
        {
            var flourCost = cookiesBaked * this.CurrentBakedGoods.CookieCost.flour
                + croissantsBaked * this.CurrentBakedGoods.CroissantCost.flour
                + cakesBaked * this.CurrentBakedGoods.CakeCost.flour;

            var butterCost = cookiesBaked * this.CurrentBakedGoods.CookieCost.butter
                + croissantsBaked * this.CurrentBakedGoods.CroissantCost.butter
                + cakesBaked * this.CurrentBakedGoods.CakeCost.butter;

            var sugarCost = cookiesBaked * this.CurrentBakedGoods.CookieCost.sugar
                + croissantsBaked * this.CurrentBakedGoods.CroissantCost.sugar
                + cakesBaked * this.CurrentBakedGoods.CakeCost.sugar;

            var moneyCost = cookiesBaked * this.CurrentBakedGoods.CookieCost.money
                + croissantsBaked * this.CurrentBakedGoods.CroissantCost.money
                + cakesBaked * this.CurrentBakedGoods.CakeCost.money;

            if (flourCost > this.CurrentResources.Flour
                || butterCost > this.CurrentResources.Butter
                || sugarCost > this.CurrentResources.Sugar
                || moneyCost > this.CurrentResources.Money)
            {
                return false;
            }

            this.CurrentResources.Money = this.CurrentResources.Money - moneyCost;
            this.CurrentResources.Butter = this.CurrentResources.Butter - butterCost;
            this.CurrentResources.Flour = this.CurrentResources.Flour - flourCost;
            this.CurrentResources.Sugar = this.CurrentResources.Sugar - sugarCost;

            this.CurrentBakedGoods.Cookies = this.CurrentBakedGoods.Cookies + cookiesBaked;
            this.CurrentBakedGoods.Croissants = this.CurrentBakedGoods.Croissants + croissantsBaked;
            this.CurrentBakedGoods.Cakes = this.CurrentBakedGoods.Cakes + cakesBaked;

            return true;
        }

        internal void SellGoods(CakeryRoom.MarketReport marketReport)
        {
            marketReport.PlayerSalesData[this] = (this.CurrentBakedGoods.Cookies, this.CurrentBakedGoods.Croissants, this.CurrentBakedGoods.Cakes);
            var profits = this.CurrentBakedGoods.Cookies * marketReport.Prices.cookiePrice
                + this.CurrentBakedGoods.Croissants * marketReport.Prices.croissantPrice
                + this.CurrentBakedGoods.Cakes * marketReport.Prices.cakePrice;

            this.CurrentResources.Money = this.CurrentResources.Money + profits;
            marketReport.PlayerProfits[this] = profits;

            this.CurrentBakedGoods.Cookies = 0;
            this.CurrentBakedGoods.Croissants = 0;
            this.CurrentBakedGoods.Cakes = 0;
        }

        internal void FinalizeUpgrades(CakeryRoom room)
        {
            // Trigger one-time effects for purchased upgrades
            foreach (var upgradePair in this.JustPurchasedUpgrades)
            {
                var upgrade = upgradePair.Value;
                var upgradeName = upgradePair.Key;

                CurrentUpgrades[upgradeName].OnPurchaseFinalized(room, upgrade.AmountOwned);
                upgrade.AmountOwned = 0;
            }

            // Trigger persistent effects for owned upgrades
            foreach (var upgrade in this.CurrentUpgrades.Values)
            {
                upgrade.OnNextRound(room);
            }
        }

        internal bool BuyUpgrades(IDictionary<string, int> upgradesBought)
        {
            // Compute total cost
            var totalCost = 0;
            foreach (var upgradeBought in upgradesBought)
            {
                if (!this.JustPurchasedUpgrades.ContainsKey(upgradeBought.Key))
                {
                    return false;
                }

                totalCost = totalCost + this.JustPurchasedUpgrades[upgradeBought.Key].Cost * upgradeBought.Value;
            }

            if (totalCost > this.CurrentResources.Money)
            {
                return false;
            }

            this.CurrentResources.Money = this.CurrentResources.Money - totalCost;

            // Finalize the upgrade purchases
            foreach (var upgradeBought in upgradesBought)
            {
                this.JustPurchasedUpgrades[upgradeBought.Key].AmountOwned = this.JustPurchasedUpgrades[upgradeBought.Key].AmountOwned + upgradeBought.Value;
            }

            return true;
        }

        internal bool UseUpgrade(string upgradeName, int amountToUse)
        {
            if (!this.CurrentUpgrades.ContainsKey(upgradeName))
            {
                return false;
            }

            var upgrade = this.CurrentUpgrades[upgradeName];
            return upgrade.OnUse(amountToUse);
        }

        internal string GetUpgradeReport()
        {
            var sb = new StringBuilder();
            foreach (var upgrade in this.CurrentUpgrades.Values)
            {
                if (upgrade.AmountOwned > 0)
                {
                    sb.Append(upgrade.OnMarketReport());
                }
            }

            return sb.ToString();
        }
    }
}
