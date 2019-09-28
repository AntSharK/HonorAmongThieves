using System.Collections.Generic;

namespace HonorAmongThieves.Cakery.GameLogic.Upgrades
{
    public abstract class Upgrade
    {
        public abstract string Name { get; }
        public abstract int Cost { get; }
        public abstract bool Usable { get; }
        public abstract (double, double, double, double) UseCost { get; }
        public abstract string Description { get; }

        public int AmountOwned = 0;
        public int AmountUsed = 0; // A negative number means this is infinitely re-usable
        public virtual string UsageEffect { get; set; } // A description of the usage of this upgrade in its current state

        protected CakeryPlayer owner;

        public Upgrade(CakeryPlayer owner)
        {
            this.owner = owner;
        }

        public virtual void OnJustPurchased(CakeryRoom room)
        {
            // Normally does nothing
        }

        public virtual void OnPurchaseFinalized(CakeryRoom room)
        {
            // Normally does nothing
        }

        public virtual void OnNextRound(CakeryRoom room)
        {
            this.AmountUsed = 0;
        }

        public virtual void OnUse(int upgradesUsed)
        {
            // Normally does nothing
            // Should check that amount used is less than amount owned
            // Should increment the "AmountUsed" number
        }

        // Initializes an empty dictionary with every single upgrade constructed
        internal static Dictionary<string, Upgrade> Initialize(CakeryPlayer player)
        {
            return new Dictionary<string, Upgrade>()
            {
                { DairyFarm.UpgradeName.ToLower(), new DairyFarm(player) },
                { SugarSubstitute.UpgradeName.ToLower(), new SugarSubstitute(player) },
            };
        }
    }
}
