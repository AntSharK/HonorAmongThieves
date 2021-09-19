namespace Necronomnomnom.Modifiers.CardModifiers
{
    /// <summary>
    /// Nullifies the effects of the card being played this round
    /// </summary>
    public class NullifyCard : CardModifier
    {
        public override void ModifyCurrentState(CardModifierState currentModifierState, Card card)
        {
            currentModifierState.DurationMultipler = 0;
            currentModifierState.DurationIncrease = 0;
            currentModifierState.DamageMultiplier = 0;
            currentModifierState.DamageIncrease = 0;

            currentModifierState.OnDmgIncChange.Add((c, m) => c.DamageIncrease = 0);
            currentModifierState.OnDmgMultChange.Add((c, m) => c.DamageMultiplier = 0);
            currentModifierState.OnDurIncChange.Add((c, m) => c.DurationIncrease = 0);
            currentModifierState.OnDurMultChange.Add((c, m) => c.DurationMultipler = 0);
        }
    }
}
