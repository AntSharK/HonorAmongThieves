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

        public int SnitchBlackmailWindow { get; private set; } = -1;

        private int Year;

        public float JailFine = 0.25f;

        public float ExtortionAmount = 0.8f;

        public Heist(string heistId, int heistCapacity, int snitchReward, int year, int snitchBlackmailWindow)
        {
            this.SnitchReward = snitchReward;
            this.Year = year;
            this.SnitchBlackmailWindow = snitchBlackmailWindow;

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
                        && this.SnitchBlackmailWindow == 0
                        && victim.BetrayalCount > 0)
                    {
                        // Victim is a snitch within an infinite window. Blackmail is successful
                        victim.Decision.WasExtortedFrom = true;
                        player.Decision.ExtortionSuccessful = true;
                    }
                    else if (victim.Decision.GoOnHeist
                        && this.SnitchBlackmailWindow > 0
                        && victim.BetrayalCount > 0
                        && victim.LastBetrayedYear + this.SnitchBlackmailWindow >= this.Year)
                    {
                        // Victim is a snitch within the window. Blackmail is successful
                        victim.Decision.WasExtortedFrom = true;
                        player.Decision.ExtortionSuccessful = true;
                    }
                    else
                    {
                        victim.Decision.WasExtortedFrom = false;
                        player.Decision.ExtortionSuccessful = false;
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

                        var jailFine = (int) (heister.NetWorth * this.JailFine);
                        heister.Decision.JailFine = jailFine;
                        heister.NetWorth = heister.NetWorth - jailFine;
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

                    var jailFine = (int)(snitcher.NetWorth * this.JailFine);
                    snitcher.Decision.JailFine = jailFine;
                    snitcher.NetWorth = snitcher.NetWorth - jailFine;
                }
            }
            else if (!heistHappens
                && !policeReported)
            {
                // Nothing goes on
            }

            // PASS 3: Compute networth transfers and Jail sentencing from Heists
            foreach (var player in this.Players.Values)
            {
                if (player.Decision.WasExtortedFrom.HasValue && player.Decision.WasExtortedFrom.Value)
                {
                    var extortedMoney = (int)(player.NetWorth * this.ExtortionAmount);
                    var extortedMoneyPerBlackmailer = extortedMoney / player.Decision.Blackmailers.Count;
                    foreach (var blackmailer in player.Decision.Blackmailers)
                    {
                        blackmailer.Decision.BlackmailReward = extortedMoneyPerBlackmailer;
                        blackmailer.NetWorth = blackmailer.NetWorth + extortedMoneyPerBlackmailer;
                    }

                    player.NetWorth = player.NetWorth - extortedMoney;
                }

                // Calculate jail punishment after distributing to extorters
                if (player.Decision.ExtortionSuccessful.HasValue && !player.Decision.ExtortionSuccessful.Value)
                {
                    // Failed to extort. Increase jail term and decrease networth
                    // If this player is already in jail, only increase his jail sentence by the minimum amount
                    if (player.Decision.JailTerm > 0)
                    {
                        player.Decision.JailTerm = player.Decision.JailTerm + player.MinJailSentence;
                    }
                    else
                    {
                        player.Decision.JailTerm = player.Decision.JailTerm + Utils.Rng.Next(player.MinJailSentence, player.MaxJailSentence);
                    }

                    if (player.Decision.JailFine > 0)
                    {
                        // Do nothing if the player has already been fined
                    }
                    else
                    {
                        // If the player has been on a successful heist, or nothing happened to his networth
                        var jailFine = (int)(player.NetWorth * this.JailFine);
                        player.Decision.JailFine = jailFine;
                        player.NetWorth = player.NetWorth - jailFine;
                    }
                }
            }

            // PASS 4: Computer jail penalties for players in jail and change status
            // And generate resolution messages
            foreach (var player in this.Players.Values)
            {
                if (player.Decision.JailTerm > 0)
                {
                    player.Decision.NextStatus = Player.Status.InJail;
                    player.YearsLeftInJail = player.Decision.JailTerm;
                    player.MinJailSentence *= 2;
                    player.MaxJailSentence *= 2;
                }

                player.Decision.FellowHeisters = heisters;
                player.GenerateFateMessage(heistHappens, policeReported, heisters);
            }
        }
    }
}
