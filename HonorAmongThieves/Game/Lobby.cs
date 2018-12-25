using HonorAmongThieves.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace HonorAmongThieves.Game
{
    public class Lobby
    {
        public Dictionary<string, Room> Rooms { get; } = new Dictionary<string, Room>();

        public static DateTime CreationTime { get; } = DateTime.UtcNow;

        public Timer Timer { get; private set; }

        public Lobby()
        {
            const int CLEANUPINTERVAL = 120000;
            this.Timer = new Timer(CLEANUPINTERVAL)
            {
                Enabled = true,
                AutoReset = true,
            };

            this.Timer.Elapsed += this.Cleanup;
        }

        public string CreateRoom(HeistHub hub, string playerName)
        {
            const int MAXLOBBYSIZE = 300;
            const int ROOMIDLENGTH = 5;

            if (!Utils.IsValidName(playerName))
            {
                return null;
            }

            var roomId = Utils.GenerateId(ROOMIDLENGTH, this.Rooms);
            if (Rooms.Values.Count < MAXLOBBYSIZE && !string.IsNullOrEmpty(roomId))
            {
                var room = new Room(roomId, hub);
                room.OwnerName = playerName;
                this.Rooms[roomId] = room;
                return roomId;
            }
            else
            {
                Console.WriteLine("MAX LOBBY SIZE REACHED: {0}", MAXLOBBYSIZE);
                return null;
            }
        }

        public Player JoinRoom(string playerName, string roomId, string connectionId)
        {
            if (!Utils.IsValidName(playerName))
            {
                return null;
            }

            if (this.Rooms.ContainsKey(roomId))
            {
                var room = this.Rooms[roomId];
                return room.CreatePlayer(playerName, connectionId);
            }

            return null;
        }

        public void Cleanup(Object source, ElapsedEventArgs e)
        {
            const int MAXROOMIDLEMINUTES = 30;
            List<string> roomsToDestroy = new List<string>();
            foreach (var room in this.Rooms)
            {
                if ((DateTime.UtcNow - room.Value.UpdatedTime).TotalMinutes > MAXROOMIDLEMINUTES)
                {
                    roomsToDestroy.Add(room.Key);
                }
            }

            foreach (var roomId in roomsToDestroy)
            {
                this.Rooms[roomId].Destroy();
                this.Rooms.Remove(roomId);
            }
        }
    }
}
