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

        public class Upgrades
        {
            // TODO (Upgrades)
        }

        public CakeryRoom Room { get; set; }

        public Status CurrentStatus { get; set; } = Status.WaitingForGameStart;

        public Resources CurrentResources { get; set; } = new Resources();
        public BakedGoods CurrentBakedGoods { get; set; } = new BakedGoods();
        public Upgrades CurrentUpgrades { get; set; } = new Upgrades(); // Does not include upgrades that are just purchased this round
        public Upgrades JustPurchasedUpgrades { get; set; } = new Upgrades(); // Upgrades take into effect only in the round after you purchase them
        public long TotalSales { get; set; } = 0;

        public CakeryPlayer(string playerName, CakeryRoom room)
            : base(playerName)
        {
            this.Room = room;
        }

        internal bool MakePurchase(int butterBought, int flourBought, int sugarBought)
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
    }
}
