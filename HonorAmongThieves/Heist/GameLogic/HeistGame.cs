﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HonorAmongThieves.Heist.GameLogic
{
    public class HeistGame
    {
        public static Dictionary<string, HeistRoom> Rooms { get; } = new Dictionary<string, HeistRoom>();

        public static DateTime CreationTime { get; } = DateTime.UtcNow;

        private const int MAXROOMIDLEMINUTES = 30;
        private const int CLEANUPINTERVAL = 120000;
        private static Timer CleanupTimer = new Timer(Cleanup, null, CLEANUPINTERVAL, CLEANUPINTERVAL);

        private IHubContext<HeistHub> hubContext;

        public HeistGame(IHubContext<HeistHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public string CreateRoom(HeistHub hub, string playerName)
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
                var room = new HeistRoom(roomId, hub, this.hubContext);
                room.OwnerName = playerName;
                Rooms[roomId] = room;
                return roomId;
            }
            else
            {
                return null;
            }
        }

        public HeistPlayer JoinRoom(string playerName, HeistRoom room, string connectionId)
        {
            if (!Utils.IsValidName(playerName))
            {
                return null;
            }

            return room.CreatePlayer(playerName, connectionId);
        }

        public static void Cleanup(object state)
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
