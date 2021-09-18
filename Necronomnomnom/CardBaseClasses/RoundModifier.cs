namespace Necronomnomnom
{
    /// <summary>
    /// Represents a modifier which acts on an entire turn in the battle, modifying the RoundModifierState
    /// </summary>
    public abstract class RoundModifier
    {
        /// <summary>
        /// Modifies the current round modifier state
        /// </summary>
        /// <param name="roundModifierState">The round modifier state to modify</param>
        /// <param name="roundState">The state of the current turn</param>
        public abstract void ModifyCurrentState(RoundModifierState roundModifierState, RoundState roundState);
    }
}
