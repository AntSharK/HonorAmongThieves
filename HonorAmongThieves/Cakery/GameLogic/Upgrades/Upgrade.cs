using System.Collections.Generic;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public abstract class Upgrade
    {
        public abstract string Name { get; }
        public abstract int Cost { get; }
        public abstract bool Usable { get; }
        public abstract string Description { get; }

        public int AmountOwned = 0;
        public int UsesLeft = 0; // A negative number means this is infinitely re-usable
        public (double money, double butter, double flour, double sugar, double cookies, double croissants, double cakes) UseCost { get; set; } = (0, 0, 0, 0, 0, 0, 0);
        public (double money, double butter, double flour, double sugar, double cookies, double croissants, double cakes) UseEffect { get; set; } = (0, 0, 0, 0, 0, 0, 0);

        protected CakeryPlayer owner;

        public Upgrade(CakeryPlayer owner)
        {
            this.owner = owner;
        }

        public virtual void OnPurchaseFinalized(CakeryRoom room, int amountPurchased)
        {
            this.AmountOwned = this.AmountOwned + amountPurchased;
        }

        public virtual void OnNextRound(CakeryRoom room)
        {
            // Reset the amount of uses
        }

        public virtual bool OnUse(int upgradesUsed)
        {
            // Normally does nothing
            // Should check that amount used is less than amount owned
            // Should increment the "UsesLeft" number
            return true;
        }

        public virtual string OnMarketReport()
        {
            // Normally returns nothing
            // Returns what this upgrade reports on the market report.
            return string.Empty;
        }

        // Initializes an empty dictionary with every single upgrade constructed
        internal static Dictionary<string, Upgrade> Initialize(CakeryPlayer player)
        {
            return new Dictionary<string, Upgrade>()
            {
                { DairyFarm.UpgradeName.ToLower(), new DairyFarm(player) },
                { SugarSubstitute.UpgradeName.ToLower(), new SugarSubstitute(player) },
                { CookieSaint.UpgradeName.ToLower(), new CookieSaint(player) },
                { CakePortal.UpgradeName.ToLower(), new CakePortal(player) },
                { PastryInterns.UpgradeName.ToLower(), new PastryInterns(player) },
            };
        }
    }
}
