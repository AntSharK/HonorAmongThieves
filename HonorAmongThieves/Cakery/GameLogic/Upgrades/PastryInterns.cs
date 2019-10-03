using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class PastryInterns : Upgrade
    {
        public const string UpgradeName = "Pastry Interns";

        public override string Name => UpgradeName;

        public override int Cost => 1000;

        public override bool Usable => false;

        public override string Description => "Lowers the Flour required for Croissants and Cakes by 10%";

        public PastryInterns(CakeryPlayer player, CakeryRoom room) : base(player, room) { }

        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);
            for (var i = 0; i < amountPurchased; i++)
            {
                this.owner.CurrentBakedGoods.CroissantCost.flour = this.owner.CurrentBakedGoods.CroissantCost.flour * 0.9;
                this.owner.CurrentBakedGoods.CakeCost.flour = this.owner.CurrentBakedGoods.CakeCost.flour * 0.9;
            }
        }

        public override string OnMarketReport()
        {
            if (this.amountJustPurchased > 0)
            {
                var percentageReduction = Math.Floor(100 - (Math.Pow(0.9, this.amountJustPurchased) * 100));
                return $"Your hiring of cheap Pastry-Making Interns has reduced the Flour required in Cakes and Croissants by {percentageReduction}%. ";
            }

            return string.Empty;
        }
    }
}
