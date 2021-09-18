using System.Collections.Generic;

namespace Necronomnomnom
{
    /// <summary>
    /// Class that stores the state for the current round
    /// A round is a single turn in a battle
    /// </summary>
    public class RoundState
    {
        public Card[] Cards;
        public int MaxCards;
        public List<CardModifier>[] CardModifiers; // CardModifiers[0] is the list of effects active at card 0
        public List<RoundModifier> RoundModifiers = new List<RoundModifier>();
        public int CurrentCardEvaluated = 0;
        public Dictionary<Player, int> DamageToMonster = new Dictionary<Player, int>();
        public Dictionary<Player, int> DamageToPlayer = new Dictionary<Player, int>();

        /// <summary>
        /// Initializes the current state of the turn
        /// </summary>
        /// <param name="maxCards">The maximum number of cards in this turn, including both player and monsters</param>
        /// <param name="players">The list of players in this round</param>
        public RoundState(int maxCards, List<Player> players)
        {
            this.MaxCards = maxCards;
            Cards = new Card[maxCards];
            CardModifiers = new List<CardModifier>[maxCards];
            for(var i = 0; i < maxCards; i++)
            {
                CardModifiers[i] = new List<CardModifier>();
            }

            foreach(var player in players)
            {
                DamageToMonster[player] = 0;
                DamageToPlayer[player] = 0;
            }
        }

        /// <summary>
        /// Performs the final evaluation of all the cards played this round
        /// </summary>
        public void EvaluateCards()
        {
            for(var i = 0; i < this.MaxCards; i++)
            {
                if (this.Cards[i] == null)
                {
                    continue;
                }

                var cardModifierState = new CardModifierState();
                foreach(var modifier in CardModifiers[i])
                {
                    modifier.ModifyCurrentState(cardModifierState, this.Cards[i]);
                }

                this.Cards[i].ActOnRound(this, cardModifierState);
            }

            var roundModifierState = new RoundModifierState();
            foreach (var modifier in RoundModifiers)
            {
                modifier.ModifyCurrentState(roundModifierState, this);
            }
        }
    }
}
