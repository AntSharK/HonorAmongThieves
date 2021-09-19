using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HonorAmongThieves
{
    /// <summary>
    /// Class representing a single game lobby
    /// There should only be a single instance of each type of room
    /// </summary>
    /// <typeparam name="HubType">The type of hub that is used</typeparam>
    /// <typeparam name="RoomType">The type of room that gets spawned</typeparam>
    /// <typeparam name="PlayerType">The type of player</typeparam>
    public abstract class Game<HubType, RoomType, PlayerType>
        where HubType : Hub
        where RoomType : Room<PlayerType, HubType>
        where PlayerType : Player
    {
        /// <summary>
        /// The maximum idle time for the room before it gets destroyed
        /// </summary>
        private const int MAXROOMIDLEMINUTES = 30;

        /// <summary>
        /// The interval that the cleanup timer runs at
        /// </summary>
        private const int CLEANUPINTERVAL = 120000;

        /// <summary>
        /// All the rooms that are of this game type
        /// </summary>
        public Dictionary<string, RoomType> Rooms { get; } = new Dictionary<string, RoomType>();

        /// <summary>
        /// The cleanup timer that runs periodically to clean rooms up
        /// </summary>
        private Timer cleanupTimer;

        /// <summary>
        /// The HubContext that gets passed in
        /// Used to create the hub to call the client
        /// </summary>
        private IHubContext<HubType> hubContext;

        /// <summary>
        /// Creates a new instance of the Game
        /// Should be a singleton instantiated using DI - should not be manually instantiated
        /// </summary>
        /// <param name="hubContext">The HubContext, injected via dependency injection</param>
        public Game(IHubContext<HubType> hubContext)
        {
            this.hubContext = hubContext;
            this.cleanupTimer = new Timer(this.Cleanup, null, CLEANUPINTERVAL, CLEANUPINTERVAL);
        }

        /// <summary>
        /// Creates a room
        /// With the creating player as the owner
        /// </summary>
        /// <param name="playerName">The creating player's name</param>
        /// <returns>The ID of the created room, or null if the room cannot be created</returns>
        public string CreateRoom(string playerName)
        {
            const int MAXLOBBYSIZE = 300;
            const int ROOMIDLENGTH = 5;

            if (!Utils.IsValidName(playerName))
            {
                return null;
            }

            var roomId = Utils.GenerateId(ROOMIDLENGTH, Rooms);
            if (Rooms.Values.Count < MAXLOBBYSIZE && !string.IsNullOrEmpty(roomId))
            {
                var room = this.InstantiateRoom(roomId, this.hubContext);
                room.OwnerName = playerName;
                Rooms[roomId] = room;
                return roomId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Instantiates an instance of a room in the game
        /// </summary>
        /// <param name="roomId">The ID of the room</param>
        /// <param name="hubContext">The HubContext for the type of hub in the room</param>
        /// <returns>An instance of a room in the game</returns>
        protected abstract RoomType InstantiateRoom(string roomId, IHubContext<HubType> hubContext);

        /// <summary>
        /// Has a player join a room
        /// </summary>
        /// <param name="playerName">The name of the joining player</param>
        /// <param name="room">The room being joined</param>
        /// <param name="connectionId">The connection ID of the joining player</param>
        /// <returns>The newly created player, or null if the player cannot join the room</returns>
        public PlayerType JoinRoom(string playerName, RoomType room, string connectionId)
        {
            if (!Utils.IsValidName(playerName))
            {
                return null;
            }

            return room.CreatePlayer(playerName, connectionId);
        }

        /// <summary>
        /// The function to cleanup idle rooms
        /// </summary>
        /// <param name="state">The state object passed in by the timer</param>
        private void Cleanup(object state)
        {
            List<string> roomsToDestroy = new List<string>();
            foreach (var room in Rooms)
            {
                if ((DateTime.UtcNow - room.Value.UpdatedTime).TotalMinutes > MAXROOMIDLEMINUTES)
                {
                    roomsToDestroy.Add(room.Key);
                }
            }

            foreach (var roomId in roomsToDestroy)
            {
                Rooms[roomId].Destroy();
                Rooms.Remove(roomId);
            }
        }
    }
}
