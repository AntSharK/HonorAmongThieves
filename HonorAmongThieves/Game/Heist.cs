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

            const int HEISTTIMEOUTDURATION = 10000;
            this.Timeout = new Timer(HEISTTIMEOUTDURATION)
            {
                AutoReset = false
            };
            this.Timeout.Elapsed += async(sender, arguments) => await this.OnTimeout(sender, arguments);
            this.Timeout.Start();

            this.Id = heistId;
        }

        public void AddPlayer(Player player)
        {
            this.Players[player.Name] = player;
        }

        private async System.Threading.Tasks.Task OnTimeout(Object o, ElapsedEventArgs e)
        {
            // Force all decisions to lock in
            foreach (var player in this.Players.Values)
            {
                player.Decision.DecisionMade = true;
                player.Decision.TimeOut = true;
                await this.Hub.HeistPrep_UpdateDecision(player);
            }

            this.TryResolve();
        }

        public bool TryResolve()
        {
            // TODO: Try to resolve the heist
            return false;
        }
    }
}
