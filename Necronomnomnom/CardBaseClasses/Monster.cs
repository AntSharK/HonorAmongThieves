using System.Collections.Generic;

namespace Necronomnomnom
{
    /// <summary>
    /// Represents an enemy of the player
    /// </summary>
    public abstract class Monster
    {
        public int HitPoints { get; set; }
        public abstract int MaxHitPoints { get; }
        public List<Card> Cards = new List<Card>();
    }
}
