using Necronomnomnom.Modifiers.CardModifiers;

namespace Necronomnomnom.Cards
{
    /// <summary>
    /// Amplifies the next few cards by a certain multiplier
    /// </summary>
    public class Amplify : Card
    {
        public int Multiplier = 2;
        public int Duration = 2;

        public override void ActOnRound(RoundState roundState, CardModifierState cardModifierState)
        {
            var totalDuration = cardModifierState.GetDuration(this.Duration);
            roundState.AddCardModifier(totalDuration, new MultiplyDuration(this.Multiplier));
            roundState.AddCardModifier(totalDuration, new MultiplyDamage(this.Multiplier));
        }
    }
}
