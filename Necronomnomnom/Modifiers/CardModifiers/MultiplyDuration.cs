namespace Necronomnomnom.Modifiers.CardModifiers
{
    /// <summary>
    /// Multiplies the duration of the current card being played
    /// </summary>
    public class MultiplyDuration : CardModifier
    {
        private int factor;

        public MultiplyDuration(int factor)
        {
            this.factor = factor;
        }

        public override void ModifyCurrentState(CardModifierState currentModifierState, Card card)
        {
            currentModifierState.DurationMultipler *= this.factor;
        }
    }
}
