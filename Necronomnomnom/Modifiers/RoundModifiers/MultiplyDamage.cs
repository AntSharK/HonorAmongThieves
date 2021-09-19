namespace Necronomnomnom.Modifiers.RoundModifiers
{
    /// <summary>
    /// Multiplies the damage being done for the entire round
    /// </summary>
    public class MultiplyDamage : RoundModifier
    {
        private int factor;

        public MultiplyDamage(int factor)
        {
            this.factor = factor;
        }

        public override void ModifyCurrentState(RoundModifierState currentModifierState, RoundState roundState)
        {
            currentModifierState.DamageMultiplier *= this.factor;
        }
    }
}
