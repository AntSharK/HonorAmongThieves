using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class PastryInterns : Upgrade
    {
        public const string UpgradeName = "Pastry Interns";

        public override string Name => UpgradeName;

        public override int Cost => 1000;

        public override bool Usable => false;

        public override string Description => "Lowers the monetary cost of production for Croissants and Cakes by 10%.";

        public PastryInterns(CakeryPlayer player) : base(player) { }

        private int amountJustPurchased = 0;
        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);
            for (var i = 0; i < amountPurchased; i++)
            {
                amountJustPurchased++;
                this.owner.CurrentBakedGoods.CroissantCost.money = this.owner.CurrentBakedGoods.CroissantCost.money * 0.9;
                this.owner.CurrentBakedGoods.CakeCost.money = this.owner.CurrentBakedGoods.CakeCost.money * 0.9;
            }
        }

        public override string OnMarketReport()
        {
            if (amountJustPurchased > 0)
            {
                var percentageReduction = Math.Floor(100 - (Math.Pow(0.9, amountJustPurchased) * 100));
                amountJustPurchased = 0;
                return $"Your hiring of cheap pastry-making interns has reduced the monetary cost of producing Cakes and Croissants by {percentageReduction}%. ";
            }

            return string.Empty;
        }
    }
}
