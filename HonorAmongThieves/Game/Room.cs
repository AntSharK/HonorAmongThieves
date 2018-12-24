using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public class Room
    {
        public bool SigningUp { get; set; } = true;

        public string Id { get; private set; }

        public List<Player> Players { get; } = new List<Player>();

        public DateTime StartTime { get; private set; }

        public DateTime CreatedTime { get; private set; }

        public DateTime UpdatedTime { get; set; }

        public int Years { get; set; } = 0;

        public List<Heist> Heists { get; } = new List<Heist>();

        public Room(string id)
        {
            this.Id = id;
            this.CreatedTime = DateTime.UtcNow;
            this.UpdatedTime = DateTime.UtcNow;
        }

        public void Destroy()
        {
            foreach (var player in this.Players)
            {
                player.CurrentStatus = Player.Status.CleaningUp;
            }

            this.SigningUp = false;
            this.Players.Clear();
            this.Heists.Clear();
        }
    }
}
