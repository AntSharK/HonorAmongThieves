using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class SugarSubstitute : Upgrade
    {
        public const string UpgradeName = "Sugar Substitute";

        public override string Name => UpgradeName;

        public override int Cost => 2000;

        public override bool Usable => false;

        public override string Description => "Lowers Sugar Usage by 20%";

        public SugarSubstitute(CakeryPlayer player, CakeryRoom room) : base(player, room) { }

        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);
            for (var i = 0; i < amountPurchased; i++)
            {
                this.owner.CurrentBakedGoods.CookieCost.sugar = this.owner.CurrentBakedGoods.CookieCost.sugar * 0.8;
                this.owner.CurrentBakedGoods.CroissantCost.sugar = this.owner.CurrentBakedGoods.CroissantCost.sugar * 0.8;
                this.owner.CurrentBakedGoods.CakeCost.sugar = this.owner.CurrentBakedGoods.CakeCost.sugar * 0.8;
            }
        }

        public override string OnMarketReport()
        {
            if (this.amountJustPurchased > 0)
            {
                var percentageReduction = Math.Floor(100 - (Math.Pow(0.8, this.amountJustPurchased) * 100));
                return $"Your research into Sugar Substitutes lowers your Sugar usage by {percentageReduction}%. ";
            }

            return string.Empty;
        }
    }
}
