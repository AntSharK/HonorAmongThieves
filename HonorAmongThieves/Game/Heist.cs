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

        public int SnitchMurderWindow { get; private set; } = -1;

        private int Year;

        public Heist(string heistId, int heistCapacity, int snitchReward, int year, int snitchMurderWindow)
        {
            this.SnitchReward = snitchReward;
            this.Year = year;
            this.SnitchMurderWindow = snitchMurderWindow;

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
            player.CurrentHeist = this;
            player.Decision = new Player.HeistDecision();
            player.Okay = false;
            player.ProjectedNetworth = player.NetWorth * Utils.Rng.Next(50, 150) / 100;
            this.Players[player.Name] = player;
        }

        public void Resolve()
        {
            var heisters = new List<Player>();

            // PASS 1: Compute resolution from blackmailing
            foreach (var player in this.Players.Values)
            {
                var victim = player.Decision.PlayerToBlackmail;
                if (victim != null)
                {
                    if (victim.Decision.Blackmailers == null)
                    {
                        victim.Decision.Blackmailers = new List<Player>();
                    }

                    victim.Decision.Blackmailers.Add(player);

                    // Resolve the blackmailing result
                    // By default, the victim is missing
                    if (!victim.Decision.ReportPolice && !victim.Decision.GoOnHeist)
                    {
                        // Victim is missing
                        victim.Decision.WasExtortedFrom = null;
                        player.Decision.ExtortionSuccessful = null;
                    }
                    else if (victim.Decision.ReportPolice)
                    {
                        // Victim is snitching this round. Blackmail is successful
                        victim.Decision.WasExtortedFrom = true;
                        player.Decision.ExtortionSuccessful = true;
                    }
                    else if (victim.Decision.GoOnHeist
                        && this.SnitchMurderWindow == 0
                        && victim.BetrayalCount > 0)
                    {
                        // Victim is a snitch within an infinite window. Blackmail is successful
                        victim.Decision.WasExtortedFrom = true;
                        player.Decision.ExtortionSuccessful = true;
                    }
                    else if (victim.Decision.GoOnHeist
                        && this.SnitchMurderWindow > 0
                        && victim.LastBetrayedYear + this.SnitchMurderWindow >= this.Year)
                    {
                        // Victim is a snitch within the window. Blackmail is successful
                        victim.Decision.WasExtortedFrom = true;
                        player.Decision.ExtortionSuccessful = true;
                    }
                    else
                    {
                        victim.Decision.WasExtortedFrom = false;
                        victim.Decision.ExtortionSuccessful = false;
                    }
                }

                if (player.Decision.GoOnHeist)
                {
                    heisters.Add(player);
                }
            }

            // PASS 2: Compute who successfully snitched
            var heistHappens = heisters.Count >= (this.Players.Count + 1) / 2;
            var successfulSnitchers = new List<Player>();
            foreach (var player in this.Players.Values)
            {
                if (player.Decision.ReportPolice
                    && player.Decision.WasExtortedFrom != true)
                {
                    successfulSnitchers.Add(player);
                }
            }

            var policeReported = false;
            if (successfulSnitchers.Count > 0)
            {
                policeReported = true;
            }

            // Compute heist rewards and jail time
            // Also apply networth changes from heists
            // Note that heisters can be blackmailed after
            if (heistHappens 
                && !policeReported 
                && heisters.Count > 0)
            {
                var reward = this.TotalReward / heisters.Count;
                foreach (var heister in heisters)
                {
                    heister.Decision.HeistReward = reward;
                    heister.NetWorth = heister.NetWorth + reward;
                }
            }
            else if (heistHappens
                && policeReported)
            {
                var snitchreward = this.SnitchReward / successfulSnitchers.Count;
                foreach (var heister in heisters)
                {
                    // Minimum jail sentence for heisters who reported to the police
                    if (heister.Decision.ReportPolice)
                    {
                        heister.Decision.JailTerm = heister.Decision.JailTerm + heister.MinJailSentence;
                    }
                    else
                    {
                        heister.Decision.JailTerm = heister.Decision.JailTerm + Utils.Rng.Next(heister.MinJailSentence, heister.MaxJailSentence);

                        var jailFine = -heister.NetWorth / 4;
                        heister.NetWorth = heister.NetWorth - jailFine;
                        heister.Decision.HeistReward = -jailFine;
                    }
                }

                foreach (var snitcher in successfulSnitchers)
                {
                    snitcher.Decision.HeistReward = snitcher.Decision.HeistReward + snitchreward;
                    snitcher.NetWorth = snitcher.NetWorth + snitchreward;
                    snitcher.BetrayalCount++;
                    snitcher.LastBetrayedYear = this.Year;
                }
            }
            else if (!heistHappens
                && policeReported)
            {
                foreach (var snitcher in successfulSnitchers)
                {
                    snitcher.Decision.JailTerm = snitcher.Decision.JailTerm + snitcher.MinJailSentence;

                    var jailFine = -snitcher.NetWorth / 4;
                    snitcher.NetWorth = snitcher.NetWorth - jailFine;
                    snitcher.Decision.HeistReward = -jailFine;
                }
            }
            else if (!heistHappens
                && !policeReported)
            {
                // Nothing goes on
            }

            // PASS 3: TODO: Compute networth transfers and Jail sentencing from Heists
            foreach (var player in this.Players.Values)
            {
                if (player.Decision.WasExtortedFrom.HasValue && player.Decision.WasExtortedFrom.Value)
                {
                    // Distribute networth to extorters
                }

                if (player.Decision.ExtortionSuccessful.HasValue && !player.Decision.ExtortionSuccessful.Value)
                {
                    // Failed to extort. Increase jail term and decrease networth
                }
            }

            // PASS 4: Computer jail penalties for players in jail and change status
            // And generate resolution messages
            foreach (var player in this.Players.Values)
            {
                if (player.Decision.JailTerm > 0)
                {
                    player.Decision.NextStatus = Player.Status.InJail;
                    player.MinJailSentence *= 2;
                    player.MaxJailSentence *= 2;
                }

                player.Decision.FellowHeisters = heisters;
                player.GenerateFateMessage(heistHappens, policeReported, heisters);
            }
        }
    }
}
