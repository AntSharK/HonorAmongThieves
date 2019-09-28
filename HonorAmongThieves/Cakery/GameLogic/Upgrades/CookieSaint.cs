namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class CookieSaint : Upgrade
    {
        public const string UpgradeName = "Cookie Saint";

        public override string Name => UpgradeName;

        public override int Cost => 500;

        public override bool Usable => true;

        public override string Description => "Converts 2 cookies into 1 croissant";

        public CookieSaint(CakeryPlayer player) : base(player) {
            this.UsageEffect = "Gain 1 Croissant.";
            this.UseCost = (0, 0, 0, 0, 2, 0, 0);
        }

        public override bool OnUse(int upgradesUsed)
        {
            if (this.AmountUsed + upgradesUsed > this.AmountOwned)
            {
                return false;
            }

            if (this.owner.CurrentBakedGoods.Cookies < upgradesUsed * 2)
            {
                return false;
            }

            this.owner.CurrentBakedGoods.Cookies = this.owner.CurrentBakedGoods.Cookies - upgradesUsed * 2;
            this.owner.CurrentBakedGoods.Croissants = this.owner.CurrentBakedGoods.Croissants + upgradesUsed;
            this.AmountUsed = this.AmountUsed + upgradesUsed;
            return true;
        }
    }
}
