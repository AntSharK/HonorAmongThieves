namespace Necronomnomnom.Cards
{
    /// <summary>
    /// Does damage to the monster
    /// </summary>
    public class DamageMonster : Card, IDamageDealingCard
    {
        public int DamageDone { get; set; } = 1;

        public DamageMonster(Player player) : base(player) { }

        public override void ActOnRound(RoundState roundState, CardModifierState cardModifierState)
        {
            var totalDamage = (this.DamageDone + cardModifierState.DamageIncrease) * cardModifierState.DamageMultiplier;
            roundState.DamageToMonster[this.owner] += totalDamage;
        }


    }
}
