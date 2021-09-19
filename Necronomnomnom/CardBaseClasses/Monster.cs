using System.Collections.Generic;

namespace Necronomnomnom
{
    /// <summary>
    /// Represents an enemy of the player
    /// </summary>
    public abstract class Monster
    {
        private int hitPoints;
        public int HitPoints
        {
            get { return this.hitPoints; }
            set
            {
                this.hitPoints = value;
                if (this.hitPoints > this.MaxHitPoints)
                {
                    this.hitPoints = this.MaxHitPoints;
                }
            }
        }
        public abstract int MaxHitPoints { get; }
        public List<Card> Cards = new List<Card>();
    }
}
