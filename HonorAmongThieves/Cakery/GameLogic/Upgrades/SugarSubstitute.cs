using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class SugarSubstitute : Upgrade
    {
        public const string UpgradeName = "Sugar Substitute";

        public override string Name => UpgradeName;

        public override int Cost => 2500;

        public override bool Usable => false;

        public override string Description => "Lowers Sugar Cost by 20%";

        public SugarSubstitute(CakeryPlayer player) : base(player) { }

        private int amountJustPurchased = 0;
        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);
            for (var i = 0; i < amountPurchased; i++)
            {
                amountJustPurchased++;
                this.owner.CurrentBakedGoods.CookieCost.sugar = this.owner.CurrentBakedGoods.CookieCost.sugar * 0.8;
                this.owner.CurrentBakedGoods.CroissantCost.sugar = this.owner.CurrentBakedGoods.CroissantCost.sugar * 0.8;
                this.owner.CurrentBakedGoods.CakeCost.sugar = this.owner.CurrentBakedGoods.CakeCost.sugar * 0.8;
            }
        }

        public override string OnMarketReport()
        {
            if (amountJustPurchased > 0)
            {
                var percentageReduction = Math.Floor(100 - (Math.Pow(0.8, amountJustPurchased) * 100));
                amountJustPurchased = 0;
                return $"Your research into Sugar Substitutes lowers your sugar usage by {percentageReduction}%. ";
            }

            return string.Empty;
        }
    }
}
