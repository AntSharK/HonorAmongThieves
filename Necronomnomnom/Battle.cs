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
        public Monster CurrentEnemy;

        public void FinishCurrentTurn()
        {
            this.CurrentTurn.EvaluateCards();

            // End the turn by dealing out damage numbers
            foreach (var damageToPlayer in this.CurrentTurn.DamageToPlayer)
            {
                damageToPlayer.Key.HitPoints -= damageToPlayer.Value;
            }

            foreach (var damageToMonster in this.CurrentTurn.DamageToMonster)
            {
                this.CurrentEnemy.HitPoints -= damageToMonster.Value;
            }
        }
    }
}
