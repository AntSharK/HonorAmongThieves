using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace HonorAmongThieves
{
    /// <summary>
    /// Represents the game lobby. Holds common game functions.
    /// </summary>
    /// <typeparam name="HubType">The type of hub being used</typeparam>
    /// <typeparam name="RoomType">The game room type</typeparam>
    /// <typeparam name="PlayerType">The player type</typeparam>
    public abstract class GameHub<HubType, RoomType, PlayerType> : Hub
        where HubType : Hub
        where RoomType : Room<PlayerType, HubType>
        where PlayerType : Player
    {
        /// <summary>
        /// The game lobby
        /// </summary>
        protected Game<HubType, RoomType, PlayerType> lobby;

        /// <inheritdoc />
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("FreshConnection");
            await base.OnConnectedAsync();
        }

        /// <inheritdoc />
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // TODO: This requires keeping track of players on a global level
            // That's not hard - but screwing up means leaking players in memory
            // Which is highly resource-intensive for a moderate reward
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Resumes a player's session when they join a game
        /// </summary>
        /// <param name="roomId">The Room ID</param>
        /// <param name="userName">The player name</param>
        /// <returns>A task</returns>
        public async Task ResumeSession(string roomId, string userName)
        {
            RoomType room;
            if (!this.lobby.Rooms.TryGetValue(roomId, out room))
            {
                await Clients.Caller.SendAsync("ClearState");
                await this.ShowError("Cannot find Room ID.");
                return;
            }

            PlayerType player;
            if (!room.Players.TryGetValue(userName, out player))
            {
                await Clients.Caller.SendAsync("ClearState");
                await this.ShowError("Cannot find player in room.");
                return;
            }

            // Transfer the connection ID
            player.ConnectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            await this.ResumePlayerSession(player);
        }

        /// <summary>
        /// Function to update the player's state
        /// </summary>
        /// <param name="player">The player</param>
        /// <returns>A task</returns>
        public abstract Task ResumePlayerSession(PlayerType player);

        /// <summary>
        /// Reconnects a player to an active game
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="room">The room to reconnect to</param>
        /// <returns>A task</returns>
        public async virtual Task ReconnectToActiveGame(PlayerType player, RoomType room)
        {
            await Clients.Caller.SendAsync("JoinRoom_ChangeState", room.Id, player.Name);
            await Clients.Caller.SendAsync("JoinRoom_TakeOverSession", room.Id, player.Name);
        }

        /// <summary>
        /// Displays an alert for the player
        /// </summary>
        /// <param name="errorMessage">The message</param>
        /// <returns>A task</returns>
        public async Task ShowError(string errorMessage)
        {
            await Clients.Caller.SendAsync("ShowError", errorMessage);
        }

        /// <summary>
        /// Creates a room in the lobby
        /// </summary>
        /// <param name="userName">The creator's name</param>
        /// <returns>A task</returns>
        public async Task CreateRoom(string userName)
        {
            var roomId = this.lobby.CreateRoom(userName);
            if (!string.IsNullOrEmpty(roomId))
            {
                await this.JoinRoom(roomId, userName);
            }
            else
            {
                if (!Utils.IsValidName(userName))
                {
                    await this.ShowError("Invalid UserName. A valid UserName is required to create a room.");
                }
                else
                {
                    var numRooms = this.lobby.Rooms.Count;
                    await this.ShowError("Unable to create room. Number of total rooms: " + numRooms);
                }
            }
        }

        /// <summary>
        /// Joins a room for a player
        /// </summary>
        /// <param name="roomId">The room id</param>
        /// <param name="userName">The player's name</param>
        /// <returns>A task</returns>
        public async Task JoinRoom(string roomId, string userName)
        {
            RoomType room;
            if (!this.lobby.Rooms.TryGetValue(roomId, out room))
            {
                // Room does not exist
                await this.ShowError("Room does not exist.");
                return;
            }

            var createdPlayer = this.lobby.JoinRoom(userName, room, Context.ConnectionId);
            if (createdPlayer != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await this.JoinRoom_UpdateView(room, createdPlayer);
            }
            else if (!room.SettingUp
                && room.Players.ContainsKey(userName))
            {
                // Take over the existing session
                await this.ResumeSession(roomId, userName);
                return;
            }
            else
            {
                await this.ShowError("Unable to create player in room. Ensure you have a valid and unique UserName.");
                return;
            }
        }

        /// <summary>
        /// Updates the view when a player joins a room
        /// </summary>
        /// <param name="room">The room</param>
        /// <param name="newPlayer">The player</param>
        /// <returns>A task</returns>
        public async Task JoinRoom_UpdateView(RoomType room, PlayerType newPlayer)
        {
            await Clients.Group(room.Id).SendAsync("JoinRoom", room.Id, newPlayer.Name);

            if (!newPlayer.IsBot)
            {
                await Clients.Caller.SendAsync("JoinRoom_ChangeState", room.Id, newPlayer.Name);
            }

            if (room.OwnerName == newPlayer.Name)
            {
                await Clients.Caller.SendAsync("JoinRoom_CreateStartButton");
            }

            var playerNames = new StringBuilder();
            foreach (var player in room.Players.Values)
            {
                playerNames.Append(player.Name);
                playerNames.Append("|");
            }

            if (playerNames.Length > 0)
            {
                playerNames.Length--;
            }

            await Clients.Group(room.Id).SendAsync("JoinRoom_UpdateState", playerNames.ToString(), newPlayer.Name);
        }
    }
}
