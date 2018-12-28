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

        public bool Resolved { get; set; } = false;

        public Heist(string heistId, int heistCapacity, int snitchReward)
        {
            this.SnitchReward = snitchReward;

            const int BASEREWARD = 30;
            const double EXPONENT = 2;
            const double EXPONENTMULTIPLIER = 3;
            var totalReward = BASEREWARD + Math.Pow(heistCapacity, EXPONENT) * EXPONENTMULTIPLIER;
            this.TotalReward = (int) (totalReward * (1 + Utils.Rng.NextDouble()));
            this.Id = heistId;
        }

        public void AddPlayer(Player player)
        {
            player.CurrentStatus = Player.Status.InHeist;
            player.Okay = false;
            this.Players[player.Name] = player;
        }
    }
}
