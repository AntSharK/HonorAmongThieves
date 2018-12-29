using System;
using System.Collections.Generic;

namespace HonorAmongThieves.Game
{
    public class Heist
    {
        public int TotalReward { get; private set; }

        public string Id { get; private set; }

        public Dictionary<string, Player> Players { get; private set; } = new Dictionary<string, Player>();

        public int SnitchReward { get; private set; } = 60;

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
            player.Decision = new Player.HeistDecision();
            player.Okay = false;
            this.Players[player.Name] = player;
        }

        public void Resolve()
        {
            var heisters = new List<Player>();
            foreach (var player in this.Players.Values)
            {
                var victim = player.Decision.PlayerToKill;
                if (victim != null)
                {
                    if (victim.Decision.Killers == null)
                    {
                        victim.Decision.Killers = new List<Player>();
                    }

                    victim.Decision.Killers.Add(player);
                }

                if (player.Decision.GoOnHeist)
                {
                    heisters.Add(player);
                }
            }

            // Calculate death
            foreach (var player in this.Players.Values)
            {
                if (player.Decision.Killers == null)
                {
                    continue;
                }

                if (player.BetrayalCount > 0
                    && (player.Decision.GoOnHeist || player.Decision.ReportPolice))
                {
                    player.Decision.NextStatus = Player.Status.Dead;
                }
                else if (player.Decision.ReportPolice)
                {
                    player.Decision.NextStatus = Player.Status.Dead;
                }

                if (player.Decision.NextStatus == Player.Status.Dead)
                {
                    var reward = player.NetWorth / player.Decision.Killers.Count;
                    foreach (var killer in player.Decision.Killers)
                    {
                        killer.NetWorth += reward;
                        killer.Decision.NetworthChange += reward;
                    }

                    player.Decision.NetworthChange = -player.NetWorth;
                    player.NetWorth = 0;
                }
            }

            var heistHappens = heisters.Count >= (this.Players.Count + 1) / 2;
            var aliveSnitchers = new List<Player>();
            foreach (var player in this.Players.Values)
            {
                if (player.Decision.ReportPolice
                    && player.Decision.NextStatus != Player.Status.Dead)
                {
                    aliveSnitchers.Add(player);
                }
            }

            var policeReported = false;
            if (aliveSnitchers.Count > 0)
            {
                policeReported = true;
            }

            // Compute rewards and jail time
            // Note that heisters can be dead
            if (heistHappens 
                && !policeReported 
                && heisters.Count > 0)
            {
                var reward = this.TotalReward / heisters.Count;
                foreach (var heister in heisters)
                {
                    heister.Decision.NetworthChange += reward;
                    heister.NetWorth += reward;
                }
            }
            else if (heistHappens
                && policeReported)
            {
                var snitchreward = this.SnitchReward / aliveSnitchers.Count;
                foreach (var heister in heisters)
                {
                    heister.Decision.NextStatus = Player.Status.InJail;
                    heister.YearsLeftInJail = Utils.Rng.Next(heister.MinJailSentence, heister.MaxJailSentence);
                }
                foreach (var snitcher in aliveSnitchers)
                {
                    snitcher.Decision.NetworthChange += snitchreward;
                    snitcher.NetWorth += snitchreward;
                    snitcher.BetrayalCount++;
                }
            }
            else if (!heistHappens
                && policeReported)
            {
                foreach (var snitcher in aliveSnitchers)
                {
                    snitcher.Decision.NextStatus = Player.Status.InJail;
                    snitcher.YearsLeftInJail = snitcher.MinJailSentence;
                }
            }
            else if (!heistHappens
                && !policeReported)
            {
                // Nothing goes on
            }

            var failedMurderers = new List<Player>();
            foreach (var player in this.Players.Values)
            {
                // Compute defensive deaths
                if (player.Decision.PlayerToKill != null
                    && player.Decision.PlayerToKill.Decision.NextStatus != Player.Status.Dead
                    && player.Decision.PlayerToKill.Decision.GoOnHeist)
                {
                    // Mark these players for death - don't resolve one by one as that might result in one player being accidentally left alive
                    failedMurderers.Add(player);
                }

                // Compute jail times
                if (player.Decision.NextStatus == Player.Status.InJail)
                {
                    var decreasedNetworth = player.NetWorth / 5;

                    // Fines are either for players caught in a heist
                    // Or snitches snitching on non-heists
                    if (!player.Decision.ReportPolice
                        || (player.Decision.ReportPolice && !heistHappens))
                    {
                        player.Decision.NetworthChange -= decreasedNetworth;
                        player.NetWorth = player.NetWorth - decreasedNetworth;
                    }

                    player.MinJailSentence *= 2;
                    player.MaxJailSentence *= 2;
                }
            }

            foreach (var player in failedMurderers)
            {
                player.Decision.NextStatus = Player.Status.Dead;
                player.Decision.Killers = new List<Player> { player };
            }

            // After generating state, compute the resolution messages
            foreach (var player in this.Players.Values)
            {
                player.Decision.HeistHappens = heistHappens;
                player.Decision.PoliceReported = policeReported;
                player.Decision.FellowHeisters = heisters;
                player.GenerateFateMessage();
            }
        }
    }
}
