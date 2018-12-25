using HonorAmongThieves.Hubs;
using System;
using System.Collections.Generic;

namespace HonorAmongThieves.Game
{
    public class Room
    {
        public bool SigningUp { get; set; } = true;

        public string Id { get; private set; }

        public string OwnerName { get; set; }

        public List<Player> Players { get; } = new List<Player>();

        public DateTime StartTime { get; private set; }

        public DateTime CreatedTime { get; private set; }

        public DateTime UpdatedTime { get; set; }

        public int Years { get; set; } = 0;

        public List<Heist> Heists { get; } = new List<Heist>();

        public HeistHub Hub { get; private set; }

        public Room(string id, HeistHub hub)
        {
            this.Id = id;
            this.Hub = hub;
            this.CreatedTime = DateTime.UtcNow;
            this.UpdatedTime = DateTime.UtcNow;
        }

        public Player CreatePlayer(string playerName, string connectionId)
        {
            const int ROOMCAPACITY = 10;
            if (this.Players.Count >= ROOMCAPACITY)
            {
                return null;
            }

            if (!this.SigningUp)
            {
                return null;
            }

            foreach (var player in this.Players)
            {
                if (player.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            var playerToAdd = new Player(playerName, this);
            playerToAdd.ConnectionId = connectionId;
            this.Players.Add(playerToAdd);
            this.UpdatedTime = DateTime.UtcNow;

            return playerToAdd;
        }

        public void Destroy()
        {
            foreach (var player in this.Players)
            {
                player.CurrentStatus = Player.Status.CleaningUp;
            }
        }
    }
}
