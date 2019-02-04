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
        public string Name { get; protected set; }

        /// <summary>
        /// Creates an instance of the player
        /// </summary>
        /// <param name="playerName">The name of the player</param>
        public Player(string playerName)
        {
            this.Name = playerName;
        }
    }
}
