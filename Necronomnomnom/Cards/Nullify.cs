using Necronomnomnom.Modifiers.CardModifiers;

namespace Necronomnomnom.Cards
{
    public class Nullify : Card
    {
        public int Duration { get; set; } = 2;

        public override void ActOnRound(RoundState roundState, CardModifierState cardModifierState)
        {
            var totalDuration = cardModifierState.GetDuration(this.Duration);
            roundState.AddCardModifier(totalDuration, new NullifyCard());
        }
    }
}
