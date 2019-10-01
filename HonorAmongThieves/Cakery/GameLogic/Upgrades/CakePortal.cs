using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class CakePortal : Upgrade
    {
        public const string UpgradeName = "Cake Portal";

        public override string Name => UpgradeName;

        public override int Cost => 1500;

        public override bool Usable => true;

        public override string Description => "Converts 1 Cake into Flour (unlimited uses). Purchase more to get more Flour (starts at 15kg) per Cake.";

        public CakePortal(CakeryPlayer player) : base(player) {
            this.UseEffect = (0, 0, 0, 0, 0, 0, 0);
            this.UseCost = (0, 0, 0, 0, 0, 0, 1);
        }

        private const double baseIncrement = 15000;
        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);

            var flourGained = this.UseEffect.flour;
            for (int i = this.AmountOwned - amountPurchased; i < this.AmountOwned; i++)
            {
                flourGained = flourGained + baseIncrement * Math.Pow(0.8, i);
            }

            this.UseEffect = (0, 0, flourGained, 0, 0, 0, 0);
        }

        public override void OnNextRound(CakeryRoom room)
        {
            base.OnNextRound(room);
            this.UsesLeft = -1; // Unlimited uses
        }

        public override bool OnUse(int upgradesUsed)
        {
            if (this.owner.CurrentBakedGoods.Cakes < upgradesUsed)
            {
                return false;
            }

            this.owner.CurrentBakedGoods.Cakes = this.owner.CurrentBakedGoods.Cakes - upgradesUsed;
            this.owner.CurrentResources.Flour = this.owner.CurrentResources.Flour + upgradesUsed * this.UseEffect.flour;
            return true;
        }

        public override string OnMarketReport()
        {
            if (this.amountJustPurchased > 0)
            {
                if (this.amountJustPurchased == this.AmountOwned)
                {
                    return $"You have discovered a portal, and after much experimentation, you find out that if you feed it a Cake, it gives you Flour. ";
                }
                else
                {
                    return $"After more research, you have increased the efficiency of your Cake-consuming portal, making it give you {Math.Round(this.UseEffect.flour)}g of Flour per Cake. ";
                }
            }

            return string.Empty;
        }
    }
}
