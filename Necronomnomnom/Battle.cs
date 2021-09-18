using System.Collections.Generic;

namespace Necronomnomnom
{
    /// <summary>
    /// Represents a battle - a single encounter in a dungeon
    /// </summary>
    public class Battle
    {
        public List<Player> Players;
        public RoundState CurrentTurn;
        public int TurnNumber = 0;
    }
}
