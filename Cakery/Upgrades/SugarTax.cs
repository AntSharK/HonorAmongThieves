using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class SugarTax : Upgrade
    {
        public const string UpgradeName = "Sugar Tax";

        public override string Name => UpgradeName;

        public override int Cost => 1250;

        public override bool Usable => false;

        public override string Description => $"Increases the price of Sugar by {Math.Floor(this.percentageIncrease * 1000)/10}% for everyone";

        private double percentageIncrease => 0.15 / this.room.Players.Count;

        public SugarTax(CakeryPlayer player, CakeryRoom room) : base(player, room) { }

        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);
            for (var i = 0; i < amountPurchased; i++)
            {
                room.CurrentPrices.Sugar = room.CurrentPrices.Sugar * (1 + this.percentageIncrease);
            }
        }

        public override string OnMarketReport()
        {
            if (this.amountJustPurchased > 0)
            {
                var totalPercentageIncrease = Math.Floor((Math.Pow(1 + this.percentageIncrease, this.amountJustPurchased) * 100 - 100));
                return $"You lobby for a sugar tax, increasing the cost of sugar for everyone by {totalPercentageIncrease}%. ";
            }

            return string.Empty;
        }
    }
}
