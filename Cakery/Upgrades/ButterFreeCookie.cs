using System;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class ButterFreeCookie : Upgrade
    {
        public const string UpgradeName = "Butter Free Cookie";

        public override string Name => UpgradeName;

        public override int Cost => 800;

        public override bool Usable => false;

        public override string Description => "Lowers the butter required to bake Cookies by 20%, increases Flour and Sugar required by 10%";

        public ButterFreeCookie(CakeryPlayer player, CakeryRoom room) : base(player, room) { }

        public override void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            base.OnPurchaseFinalized(room, amountPurchased);
            for (var i = 0; i < amountPurchased; i++)
            {
                this.owner.CurrentBakedGoods.CookieCost.butter = this.owner.CurrentBakedGoods.CookieCost.butter * 0.8;
                this.owner.CurrentBakedGoods.CookieCost.flour = this.owner.CurrentBakedGoods.CookieCost.flour * 1.1;
                this.owner.CurrentBakedGoods.CookieCost.sugar = this.owner.CurrentBakedGoods.CookieCost.sugar * 1.1;
            }
        }

        public override string OnMarketReport()
        {
            if (this.amountJustPurchased > 0)
            {
                return $"You mix up your cookie-production formula. Cookies now cost {Math.Ceiling(this.owner.CurrentBakedGoods.CookieCost.butter)}g Butter, {Math.Ceiling(this.owner.CurrentBakedGoods.CookieCost.flour)}g Flour and {Math.Ceiling(this.owner.CurrentBakedGoods.CookieCost.sugar)}g Sugar. ";
            }

            return string.Empty;
        }
    }
}
