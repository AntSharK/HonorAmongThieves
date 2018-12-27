using HonorAmongThieves.Hubs;
using System;
using System.Collections.Generic;
using System.Timers;

namespace HonorAmongThieves.Game
{
    public class Heist
    {
        public int TotalReward { get; private set; }

        public string Id { get; private set; }

        public Dictionary<string, Player> Players { get; private set; } = new Dictionary<string, Player>();

        public Dictionary<Player, HeistDecision> Decisions { get; private set; } = new Dictionary<Player, HeistDecision>();

        public int SnitchReward { get; private set; } = 60;

        private Timer Timeout;

        private HeistHub Hub;

        public Heist(string heistId, int heistCapacity, int snitchReward, HeistHub hub)
        {
            this.Hub = hub;
            this.SnitchReward = snitchReward;

            const int BASEREWARD = 30;
            const double EXPONENT = 2;
            const double EXPONENTMULTIPLIER = 3;
            var totalReward = BASEREWARD + Math.Pow(heistCapacity, EXPONENT) * EXPONENTMULTIPLIER;
            this.TotalReward = (int) (totalReward * (1 + Utils.Rng.NextDouble()));

            const int HEISTTIMEOUTDURATION = 100000;
            this.Timeout = new Timer(HEISTTIMEOUTDURATION)
            {
                AutoReset = false
            };
            this.Timeout.Elapsed += this.OnTimeout;
            this.Timeout.Start();

            this.Id = heistId;
        }

        public void AddPlayer(Player player)
        {
            this.Players[player.Name] = player;
            this.Decisions[player] = new HeistDecision();
        }

        private void OnTimeout(Object o, ElapsedEventArgs e)
        {
            // Force all decisions to lock in
            foreach (var decision in this.Decisions.Values)
            {
                decision.DecisionMade = true;
            }

            this.TryResolve();
        }

        public void MakeDecision(Player player)
        {
            // TODO: Change decision made by player
        }

        public bool TryResolve()
        {
            // TODO: Try to resolve the heist
            return false;
        }

        public class HeistDecision
        {
            public bool DecisionMade { get; set; } = false;
            public bool GoOnHeist { get; set; } = false;
            public bool ReportPolice { get; set; } = false;
            public Player PlayerToKill { get; set; } = null;
        }
    }
}
