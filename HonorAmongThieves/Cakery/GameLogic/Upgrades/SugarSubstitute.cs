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

        public override void OnPurchaseFinalized(CakeryRoom room)
        {
            for (var i = 0; i < this.AmountOwned; i++)
            {
                this.owner.CurrentBakedGoods.CookieCost.butter = this.owner.CurrentBakedGoods.CookieCost.butter * 0.8;
                this.owner.CurrentBakedGoods.CroissantCost.butter = this.owner.CurrentBakedGoods.CroissantCost.butter * 0.8;
                this.owner.CurrentBakedGoods.CakeCost.butter = this.owner.CurrentBakedGoods.CakeCost.butter * 0.8;
            }
        }
    }
}
