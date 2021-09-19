namespace Necronomnomnom.Modifiers.CardModifiers
{
    /// <summary>
    /// Multiplies the damage being done of the current card being played
    /// </summary>
    public class MultiplyDamage : CardModifier
    {
        private int factor;

        public MultiplyDamage(int factor)
        {
            this.factor = factor;
        }

        public override void ModifyCurrentState(CardModifierState currentModifierState, Card card)
        {
            currentModifierState.DamageMultiplier *= this.factor;
        }
    }
}
