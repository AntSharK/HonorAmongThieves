using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;

namespace HonorAmongThieves
{
    /// <summary>
    /// An instance of a game
    /// </summary>
    public abstract class Room<PlayerType, HubType>
        where PlayerType : Player
        where HubType : Hub
    {
        /// <summary>
        /// Whether the room is still allowing players to join
        /// </summary>
        public bool SettingUp { get; set; } = true;

        /// <summary>
        /// The unique ID of the room
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The name of the room's creator
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// All the players in the room
        /// </summary>
        public Dictionary<string, PlayerType> Players { get; } = new Dictionary<string, PlayerType>();

        /// <summary>
        /// The last time this room was updated
        /// </summary>
        public DateTime UpdatedTime { get; protected set; } = DateTime.UtcNow;

        /// <summary>
        /// The hub context to send messages to the room
        /// </summary>
        private IHubContext<HubType> hubContext;

        /// <summary>
        /// Creates an instance of this room
        /// </summary>
        /// <param name="id">The ID of the room</param>
        /// <param name="hubContext">The HubContext for this room</param>
        public Room(string id, IHubContext<HubType> hubContext)
        {
            this.Id = id;
            this.hubContext = hubContext;
        }

        /// <summary>
        /// Function called to create a player
        /// </summary>
        /// <param name="playerName">The name of the player</param>
        /// <param name="connectionId">The connection ID of the player</param>
        /// <returns>The newly created player, or null if the player cannot be created</returns>
        public PlayerType CreatePlayer(string playerName, string connectionId)
        {
            const int ROOMCAPACITY = 20;
            if (this.Players.Count >= ROOMCAPACITY)
            {
                return null;
            }

            if (!this.SettingUp)
            {
                return null;
            }

            if (this.Players.ContainsKey(playerName))
            {
                return null;
            }

            var playerToAdd = this.InstantiatePlayer(playerName);
            playerToAdd.ConnectionId = connectionId;
            this.Players[playerName] = playerToAdd;
            this.UpdatedTime = DateTime.UtcNow;

            return playerToAdd;
        }

        /// <summary>
        /// Creates an instance of the player with the correct type
        /// </summary>
        /// <param name="playerName">The name of the player</param>
        /// <returns>An instance of the player</returns>
        protected abstract PlayerType InstantiatePlayer(string playerName);

        /// <summary>
        /// The function to call when disposing of this room
        /// </summary>
        public abstract void Destroy();
    }
}
