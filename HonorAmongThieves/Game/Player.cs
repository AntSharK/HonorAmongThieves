using System;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public class Player
    {
        public Room Room { get; set; }

        public string Name { get; private set; }

        public int NetWorth { get; set; } = 10;

        public int TimeSpentInJail { get; set; } = 0;

        public int YearsLeftInJail { get; set; } = -1;

        public int MinJailSentence { get; set; } = 1;

        public int MaxJailSentence { get; set; } = 2;

        public int BetrayalCount { get; set; } = 0;

        public Status CurrentStatus { get; set; }

        public Status NextStatus { get; set; }

        public DateTime LastUpdate { get; private set; }

        public string ConnectionId { get; set; }

        public HeistDecision Decision { get; set; } = new HeistDecision();

        public bool Okay { get; set; } = true;

        public Player(string name, Room room)
        {
            this.Name = name;
            this.Room = room;
            this.CurrentStatus = Status.WaitingForGameStart;
            this.LastUpdate = DateTime.UtcNow;
        }

        public void MurderDecision(Player victim)
        {
            this.Decision.DecisionMade = true;
            this.Decision.GoOnHeist = true;
            this.Decision.ReportPolice = false;
            this.Decision.PlayerToKill = victim;

            this.Okay = true;
            this.CurrentStatus = Status.HeistDecisionMade;
        }

        public void MakeDecision(bool turnUp, bool snitch)
        {
            this.Decision.DecisionMade = true;
            this.Decision.GoOnHeist = turnUp;
            this.Decision.ReportPolice = snitch;

            this.Okay = true;
            this.CurrentStatus = Status.HeistDecisionMade;
        }

        public enum Status
        {
            WaitingForGameStart,
            FindingHeist,
            InHeist,
            HeistDecisionMade,

            // Non-action states
            InJail,
            Dead,
            CleaningUp
        }

        public class HeistDecision
        {
            public bool DecisionMade { get; set; } = false;
            public bool GoOnHeist { get; set; } = true;
            public bool ReportPolice { get; set; } = false;
            public Player PlayerToKill { get; set; } = null;
        }
    }
}
