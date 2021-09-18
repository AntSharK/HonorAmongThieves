namespace Necronomnomnom
{
    /// <summary>
    /// A card held by a player
    /// </summary>
    public abstract class Card
    {
        protected Player owner;
        public Card(Player owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Has the card act on the round state
        /// </summary>
        /// <param name="roundState">The current round state</param>
        /// <param name="cardModifierState">The final card modifier state to modify this card</param>
        public abstract void ActOnRound(RoundState roundState, CardModifierState cardModifierState);

        /// <summary>
        /// Upgrades the current card
        /// </summary>
        public virtual void Upgrade()
        {
        }
    }
}
