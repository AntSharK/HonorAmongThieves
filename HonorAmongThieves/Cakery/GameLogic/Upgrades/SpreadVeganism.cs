using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class SpreadVeganism : Upgrade
    {
        public const string UpgradeName = "Spread Veganism";

        public override string Name => UpgradeName;

        public override int Cost => 1350;

        public override bool Usable => false;

        public override string Description => $"Increases the price of Butter by {Math.Floor(this.percentageIncrease * 1000)/10}%.";

        private double percentageIncrease => 0.15 / this.room.Players.Count;

        public SpreadVeganism(CakeryPlayer player, CakeryRoom room) : base(player, room) { }

        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);
            for (var i = 0; i < amountPurchased; i++)
            {
                room.CurrentPrices.Butter = room.CurrentPrices.Butter * (1 + this.percentageIncrease);
            }
        }

        public override string OnMarketReport()
        {
            if (this.amountJustPurchased > 0)
            {
                var totalPercentageIncrease = Math.Floor((Math.Pow(1 + this.percentageIncrease, this.amountJustPurchased) * 100 - 100));
                return $"You paste signs to support humane cow treatment, increasing the price of butter by {totalPercentageIncrease}%. ";
            }

            return string.Empty;
        }
    }
}
