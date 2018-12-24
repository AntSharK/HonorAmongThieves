using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public class Player
    {
        public Room Room { get; set; }

        public string Name { get; private set; }

        public int NetWorth { get; set; } = 0;

        public int YearsLeftInJail { get; set; } = -1;

        public bool HasBetrayed { get; set; } = false;

        public Status CurrentStatus { get; set; }

        public DateTime LastUpdate { get; private set; }

        public string ConnectionId { get; set; }

        public Player(string name, Room room)
        {
            this.Name = name;
            this.Room = room;
            this.CurrentStatus = Status.WaitingForGameStart;
            this.LastUpdate = DateTime.UtcNow;
        }

        public enum Status
        {
            WaitingForGameStart,
            FindingHeist,
            InHeist,
            InJail,
            Dead,
            Retired,
            CleaningUp
        }
    }
}
