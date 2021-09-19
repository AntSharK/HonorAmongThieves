namespace Necronomnomnom.Cards
{
    /// <summary>
    /// Does damage to the monster
    /// </summary>
    public class DamageMonster : Card, IDamageDealingCard
    {
        public int DamageDone { get; set; } = 1;

        public override void ActOnRound(RoundState roundState, CardModifierState cardModifierState)
        {
            var totalDamage = cardModifierState.GetDamageDone(this.DamageDone);
            roundState.DamageToMonster[this.Owner] += totalDamage;
        }
    }
}
