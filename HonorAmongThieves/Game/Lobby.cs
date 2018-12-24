using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public class Lobby
    {
        public Dictionary<string, Room> Rooms { get; } = new Dictionary<string, Room>();

        public static DateTime CreationTime { get; } = DateTime.UtcNow;

        public const int ROOMCAPACITY = 10;

        public string CreateRoom()
        {
            const int MAXLOBBYSIZE = 300;
            const int ROOMIDLENGTH = 5;

            if (Rooms.Values.Count < MAXLOBBYSIZE)
            {
                string roomId = null;
                while (roomId == null || Rooms.ContainsKey(roomId))
                {
                    roomId = Guid.NewGuid().ToString().Substring(0, ROOMIDLENGTH);
                }

                var room = new Room(roomId);
                this.Rooms[roomId] = room;
                return roomId;
            }

            else
            {
                Console.WriteLine("MAX LOBBY SIZE REACHED: {0}", MAXLOBBYSIZE);
                return null;
            }
        }

        public bool JoinRoom(string playerName, string roomId, string connectionId)
        {
            if (playerName.Length == 0
                || playerName.Length > 10)
            {
                return false;
            }

            foreach (char c in playerName.ToCharArray())
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return false;
                }
            }

            if (this.Rooms.ContainsKey(roomId))
            {
                var room = this.Rooms[roomId];
                if (room.Players.Count >= ROOMCAPACITY)
                {
                    return false;
                }

                if (!room.SigningUp)
                {
                    return false;
                }

                foreach (var roomPlayer in room.Players)
                {
                    if (roomPlayer.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                var player = new Player(playerName, room);
                player.ConnectionId = connectionId;
                room.Players.Add(player);
                return true;
            }

            return false;
        }

        public void Cleanup()
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
                this.Rooms[roomId] = null;
            }
        }
    }
}
