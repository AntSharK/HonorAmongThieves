namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public class DairyFarm : Upgrade
    {
        public const string UpgradeName = "Dairy Farm";

        public override string Name => UpgradeName;

        public override int Cost => 5000;

        public override bool Usable => false;

        public override (double, double, double, double) UseCost => (0, 0, 0, 0);

        public override string Description => "Generates 1kg of butter per year";

        public DairyFarm(CakeryPlayer player) : base(player) { }

        public override void OnNextRound(CakeryRoom room)
        {
            this.owner.CurrentResources.Butter = this.owner.CurrentResources.Butter + (1000 * this.AmountOwned);
        }
    }
}
