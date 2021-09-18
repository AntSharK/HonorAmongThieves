namespace Necronomnomnom
{
    /// <summary>
    /// Represents a modifier which acts on a card, modifying the CardModifierState
    /// </summary>
    public abstract class CardModifier
    {
        /// <summary>
        /// Modifies the Card Modifier State
        /// </summary>
        /// <param name="currentModifierState">The current modifications to apply to a card</param>
        /// <param name="card">The card being modified - some modifiers will only apply to certain classes of cards</param>
        public abstract void ModifyCurrentState(CardModifierState currentModifierState, Card card);
    }
}
