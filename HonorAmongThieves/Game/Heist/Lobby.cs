using HonorAmongThieves.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HonorAmongThieves.Game.Heist
{
    public class Lobby
    {
        public static Dictionary<string, Room> Rooms { get; } = new Dictionary<string, Room>();

        public static DateTime CreationTime { get; } = DateTime.UtcNow;

        private const int CLEANUPINTERVAL = 1200;//00;
        private static Timer CleanupTimer = new Timer(Cleanup, null, CLEANUPINTERVAL, CLEANUPINTERVAL);

        public IHubContext<HeistHub> hubContext;

        public Lobby(IHubContext<HeistHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public static string CreateRoom(HeistHub hub, string playerName)
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
                var room = new Room(roomId, hub);
                room.OwnerName = playerName;
                Rooms[roomId] = room;
                return roomId;
            }
            else
            {
                return null;
            }
        }

        public static Player JoinRoom(string playerName, Room room, string connectionId)
        {
            if (!Utils.IsValidName(playerName))
            {
                return null;
            }

            return room.CreatePlayer(playerName, connectionId);
        }

        public static void Cleanup(object state)
        {
            const int MAXROOMIDLEMINUTES = 30;
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
