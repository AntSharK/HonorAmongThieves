namespace HonorAmongThieves.GameLogic
{
    /// <summary>
    /// Class representing a player
    /// </summary>
    public abstract class Player
    {
        /// <summary>
        /// The SignalR Connection ID tied to this player
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// The player's name
        /// Should be unique to the room
        /// </summary>
        public string Name { get; set; }
    }
}
