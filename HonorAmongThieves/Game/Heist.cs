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

        public List<Player> Heisters { get; set; } = new List<Player>();

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
            var snitchers = new List<Player>(); // All players who are snitching
            var abandoners = new List<Player>(); // All players who are not snitching and not going on the heist
            var victimMurdererMap = new Dictionary<Player, List<Player>>();

            foreach (var player in this.Players.Values)
            {
                var victim = player.Decision.PlayerToKill;
                if (player.Decision.PlayerToKill != null
                    && (victim.Decision.GoOnHeist
                    || victim.Decision.ReportPolice))
                {
                    if (!victimMurdererMap.ContainsKey(victim))
                    {
                        victimMurdererMap[victim] = new List<Player>();
                    }

                    victimMurdererMap[victim].Add(player);
                }

                if (player.Decision.GoOnHeist)
                {
                    this.Heisters.Add(player);
                }

                if (!player.Decision.GoOnHeist
                    && !player.Decision.ReportPolice)
                {
                    abandoners.Add(player);
                }
            }

            // First to resolve - kill all snitches that aren't in the heist
            foreach (var victim in victimMurdererMap.Keys)
            {
                if (victim.Decision.ReportPolice
                    && !victim.Decision.GoOnHeist)
                {
                    victim.CurrentStatus = Player.Status.Dead;
                    victim.Decision.FateTitle = "KILLED";
                    victim.Decision.FateDescription = "You keep a close eye on the ongoing heist, with the intention of reporting the whereabouts to the police. Then all of a sudden, you encounter a curious bullet to the brain.";
                }

                var reward = victim.NetWorth / victimMurdererMap[victim].Count;
                foreach (var murderer in victimMurdererMap[victim])
                {
                    murderer.NetWorth += reward;
                    murderer.Decision.FateTitle = "KILLED THE SNITCH + ";
                    murderer.Decision.FateDescription = "You catch " + victim.Name + " lurking outside the heist gathering area talking to the police.";
                    if (victimMurdererMap[victim].Count > 1)
                    {
                        murderer.Decision.FateDescription += " Before you can put him down, a shot rings out in the air. " + victim.Name + " falls to the ground, dead. You wait a few minutes, then sneak up to his body and steal $" + reward + "million. "; 
                    }
                    else
                    {
                        murderer.Decision.FateDescription += " You put a bullet into the back of his skull. You also find the password to his bank account, and instantly gain $" + reward + " million. ";
                    }
                }

                victim.NetWorth = 0;
            }

            var heistOccurs = this.Heisters.Count >= (this.Players.Count + 1) / 2;
            foreach (var player in this.Players.Values)
            {
                if (player.CurrentStatus != Player.Status.Dead
                    && player.Decision.ReportPolice)
                {
                    snitchers.Add(player);
                }
            }

            foreach (var player in Heisters)
            {
                player.Decision.FellowHeisters = this.Heisters;
            }

            // If heist does not occur, this is the easiest to resolve
            if (!heistOccurs)
            {
                // Resolve killings
                foreach (var victim in victimMurdererMap.Keys)
                {
                    // Easiest case: victim is innocent
                    if (victim.BetrayalCount == 0)
                        // && !victim.Decision.ReportPolice) - Actually, if he intends to be a snitch but nothing happens, all is good
                    {
                        foreach (var murderer in victimMurdererMap[victim])
                        {
                            murderer.CurrentStatus = Player.Status.Dead;
                            murderer.Decision.FateTitle += " KILLED";
                            murderer.Decision.FateDescription += " You confront " + victim.Name + " about being a snitch. But the argument gets a bit too heated, and you lose your head. Literally. Via chainsaw.";
                        }

                        victim.Decision.FateTitle += " CONFRONTED";

                        if (victimMurdererMap[victim].Count > 0)
                        {
                            victim.Decision.FateDescription += " You were confronted for being a snitch. Despite your innocence, the argument got heated. And some people died. But not you.";
                        }
                        else
                        {
                            victim.Decision.FateDescription += " You were confronted for being a snitch. Despite your innocence, the argument got heated. And the accuser died.";
                        }
                    }

                    // Victim is not innocent
                    if (victim.BetrayalCount > 0)
                    {
                        // Copy and pasted from above
                        victim.CurrentStatus = Player.Status.Dead;
                        victim.Decision.FateTitle = "KILLED";
                        victim.Decision.FateDescription = "You keep a close eye on the ongoing heist, with the intention of reporting the whereabouts to the police. Then all of a sudden, you encounter a curious bullet to the brain.";
                    }

                    var reward = victim.NetWorth / victimMurdererMap[victim].Count;
                    foreach (var murderer in victimMurdererMap[victim])
                    {
                        murderer.NetWorth += reward;
                        murderer.Decision.FateTitle = "KILLED THE SNITCH + ";
                        murderer.Decision.FateDescription = "You catch " + victim.Name + " lurking outside the heist gathering area talking to the police.";
                        if (victimMurdererMap[victim].Count > 1)
                        {
                            murderer.Decision.FateDescription += " Before you can put him down, a shot rings out in the air. " + victim.Name + " falls to the ground, dead. You wait a few minutes, then sneak up to his body and steal $" + reward + "million. ";
                        }
                        else
                        {
                            murderer.Decision.FateDescription += " You put a bullet into the back of his skull. You also find the password to his bank account, and instantly gain $" + reward + " million. ";
                        }
                    }
                }

                // Resolve all heisters
                foreach (var heister in Heisters)
                {
                    if (heister.CurrentStatus != Player.Status.Dead
                        && !heister.Decision.ReportPolice)
                    {
                        heister.Decision.FateTitle += " LACK OF ATTENDANCE";
                        heister.Decision.FateDescription += " Unfortunately, not enough people show up to try to go on the heist. Oh well, better luck next time.";
                    }
                }

                // Punish all snitchers
                foreach (var snitcher in snitchers)
                {
                    if (snitcher.CurrentStatus != Player.Status.Dead)
                        // && snitcher.Decision.GoOnHeist) - Consider dialog alternatives for when the snitch doesn't go on the heist
                    {
                        snitcher.Decision.FateTitle += " FAILED TO SNITCH";
                        snitcher.Decision.FateDescription = " Unfortunately, there weren't enough thieves to carry out the heist. Now you feel stupid and get tossed into jail for wasting the police's time.";
                        snitcher.CurrentStatus = Player.Status.InJail;
                    }
                }
            }
            // If the heist occurs
            else
            {
                if (snitchers.Count > 0)
                {
                    // First, resolve the snitching reward
                    foreach (var snitcher in snitchers)
                    {
                        if (snitcher.CurrentStatus != Player.Status.Dead)
                        {

                        }

                        if (snitcher.Decision.GoOnHeist)
                        {
                            snitcher.CurrentStatus = Player.Status.InJail;
                        }
                    }
                }
                else
                {

                }
                // Finally, resolve killings
            }

            foreach (var player in abandoners)
            {
                player.Decision.FateTitle = "LAZE AROUND";
                player.Decision.FateDescription = "You spend your free time lazing around, waiting for the next, better opportunity.";
            }

            // Resolve jail for all
            foreach (var player in this.Players.Values)
            {
                if (player.CurrentStatus == Player.Status.InJail)
                {
                    if (player.Decision.ReportPolice)
                    {
                        player.YearsLeftInJail += player.MinJailSentence;
                    }
                    else
                    {
                        player.YearsLeftInJail = Utils.Rng.Next(player.MinJailSentence, player.MaxJailSentence);
                        player.NetWorth = player.NetWorth * 8 / 10;
                    }

                    player.MinJailSentence *= 2;
                    player.MaxJailSentence *= 2;
                }
            }
        }
    }
}
