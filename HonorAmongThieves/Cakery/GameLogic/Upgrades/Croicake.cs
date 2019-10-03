namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class CroiCake : Upgrade
    {
        public const string UpgradeName = "CroiCake";

        public override string Name => UpgradeName;

        public override int Cost => 500;

        public override bool Usable => true;

        public override string Description => "Converts 1 Cake into 9 Croissants (1 use)";

        public CroiCake(CakeryPlayer player, CakeryRoom room) : base(player, room) {
            this.UseEffect = (0, 0, 0, 0, 0, 9, 0);
            this.UseCost = (0, 0, 0, 0, 0, 0, 1);
        }

        public override void OnNextRound(CakeryRoom room)
        {
            base.OnNextRound(room);
            this.UsesLeft = this.AmountOwned * 1; // 1 use per purchase
        }

        public override bool OnUse(int upgradesUsed)
        {
            if (this.UsesLeft - upgradesUsed < 0)
            {
                return false;
            }

            if (this.owner.CurrentBakedGoods.Cakes < upgradesUsed * 1)
            {
                return false;
            }

            this.owner.CurrentBakedGoods.Cakes = this.owner.CurrentBakedGoods.Cakes - upgradesUsed * 1;
            this.owner.CurrentBakedGoods.Croissants = this.owner.CurrentBakedGoods.Croissants + upgradesUsed * 9;
            this.UsesLeft = this.UsesLeft - upgradesUsed;
            return true;
        }

        public override string OnMarketReport()
        {            
            if (this.amountJustPurchased > 0)
            {
                if (this.amountJustPurchased == this.AmountOwned)
                {
                    return $"You have discovered the technique of creating Croissants from Cakes, and dub this new creation the 'CroiCake' (prounounced 'Krike'). ";
                }
                else
                {
                    return $"You buy more CroiCake machines, letting you to produce {this.amountJustPurchased * this.UseEffect.croissants} more Croissant-Cakes per year. ";
                }
            }

            return string.Empty;
        }
    }
}
