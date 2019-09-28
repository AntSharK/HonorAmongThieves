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
            // Normally does nothing
        }

        public virtual void OnUse()
        {
            // Normally does nothing
        }

        // Initializes an empty dictionary with every single upgrade constructed
        internal static Dictionary<string, Upgrade> Initialize(CakeryPlayer player)
        {
            return new Dictionary<string, Upgrade>()
            {
                { DairyFarm.UpgradeName, new DairyFarm(player) },
                { SugarSubstitute.UpgradeName, new SugarSubstitute(player) },
            };
        }
    }
}
