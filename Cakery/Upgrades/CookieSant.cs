namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class CookieSant : Upgrade
    {
        public const string UpgradeName = "CookieSant";

        public override string Name => UpgradeName;

        public override int Cost => 200;

        public override bool Usable => true;

        public override string Description => "Converts 2 Cookies into 1 Croissant (2 uses)";

        public CookieSant(CakeryPlayer player, CakeryRoom room) : base(player, room) {
            this.UseEffect = (0, 0, 0, 0, 0, 1, 0);
            this.UseCost = (0, 0, 0, 0, 2, 0, 0);
        }

        public override void OnNextRound(CakeryRoom room)
        {
            base.OnNextRound(room);
            this.UsesLeft = this.AmountOwned * 2; // 2 uses per purchase
        }

        public override bool OnUse(int upgradesUsed)
        {
            if (this.UsesLeft - upgradesUsed < 0)
            {
                return false;
            }

            if (this.owner.CurrentBakedGoods.Cookies < upgradesUsed * 2)
            {
                return false;
            }

            this.owner.CurrentBakedGoods.Cookies = this.owner.CurrentBakedGoods.Cookies - upgradesUsed * 2;
            this.owner.CurrentBakedGoods.Croissants = this.owner.CurrentBakedGoods.Croissants + upgradesUsed;
            this.UsesLeft = this.UsesLeft - upgradesUsed;
            return true;
        }

        public override string OnMarketReport()
        {            
            if (this.amountJustPurchased > 0)
            {
                if (this.amountJustPurchased == this.AmountOwned)
                {
                    return $"You have discovered the technique of creating Croissants from Cookies, and dub this new creation the 'Cookiesant'. ";
                }
                else
                {
                    return $"You buy more Cookiesant machines, letting you to produce {this.amountJustPurchased * this.UseEffect.croissants} more Cookie-Croissants per year. ";
                }
            }

            return string.Empty;
        }
    }
}
