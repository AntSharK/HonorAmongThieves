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

        public Amplify(Player player) : base(player) { }

        public override void ActOnRound(RoundState roundState, CardModifierState cardModifierState)
        {
            var totalDuration = (this.Duration + cardModifierState.DurationIncrease) * cardModifierState.DurationMultipler;
            for (var i = 0; i < totalDuration; i++)
            {
                var roundNum = i + roundState.CurrentCardEvaluated;
                if (roundNum >= roundState.MaxCards)
                {
                    break;
                }

                roundState.CardModifiers[roundNum].Add(new MultiplyDuration(this.Multiplier));
                roundState.CardModifiers[roundNum].Add(new MultiplyDamage(this.Multiplier));
            }
        }
    }
}
