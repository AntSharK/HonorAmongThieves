using System;
using System.Collections.Generic;
using System.Text;

namespace HonorAmongThieves.Game
{
    public static class TextGenerator
    {
        public static Tuple<string, string> NoHeists
        {
            get
            {
                return Tuple.Create("FINDING HEISTS...", "Your contacts don't seem to be responding. If there is any crime going on, you're not being invited.");
            }
        }

        public static Tuple<string, string> InJail
        {
            get
            {
                return Tuple.Create("IN JAIL", "You're IN JAIL. Years left: {0}.");
            }
        }

        public static Tuple<string, string> FreeFromJail
        {
            get
            {
                return Tuple.Create("FREE AT LAST", "You're finally out of jail! A new person! Free from the life of crime!");
            }
        }

        public static Tuple<string, string> StillInJail
        {
            get
            {
                return Tuple.Create("STILL IN JAIL", "You're still in jail for another {0} year(s).");
            }
        }

        public static Tuple<string, string> VacationEnded
        {
            get
            {
                return Tuple.Create("WAITING", "You wait around, and a year passes you by without anything happening.");
            }
        }

        public static Tuple<string, string> GenerateFateMessage(bool heistHappens, bool policeReported, Player.HeistDecision decision)
        {
            // Resolve not going on the heist but getting blackmailed while snitching
            if (decision.WasExtortedFrom.HasValue
                && decision.GoOnHeist
                && decision.WasExtortedFrom.Value)
            {
                return GetBlackmailedMessage(decision);
            }

            // Resolve doing nothing
            if (!decision.GoOnHeist
                && !decision.ReportPolice)
            {
                return Bailing;
            }

            // Resolve blackmail not finding the target
            if (decision.ExtortionSuccessful == null
                && decision.PlayerToBlackmail != null)
            {
                return BlackmaileeMissingMessage(decision);
            }

            // Resolve blackmailing a snitch before he snitched and doesn't go on a heist
            if (decision.ExtortionSuccessful.HasValue
                && decision.ExtortionSuccessful.Value
                && decision.PlayerToBlackmail.Decision.GoOnHeist
                && decision.PlayerToBlackmail.Decision.ReportPolice)
            {
                return BlackmailSnitchMessage(decision);
            }

            var fateTitle = "";
            var fateMessage = "";

            // Resolve heist status
            if (decision.GoOnHeist)
            {
                if (!heistHappens)
                {
                    fateTitle = fateTitle + "NOT ENOUGH ATTENDENCE. ";
                    fateMessage = fateMessage + HeistAbandoned(decision.FellowHeisters);
                }
                else if (heistHappens
                    && policeReported
                    && !decision.ReportPolice)
                {
                    fateTitle = fateTitle + "ARRESTED. ";
                    fateMessage = fateMessage + HeistArrested;
                }
                else if (heistHappens
                    && !policeReported)
                {
                    fateTitle = fateTitle + "SUCCESSFUL HEIST. ";
                    fateMessage = fateMessage + HeistSuccess(decision);
                }
            }

            // Resolve snitching status - only for when snitching is successful and the snitch isn't blackmailed
            if (decision.ReportPolice
                && policeReported
                && (!decision.WasExtortedFrom.HasValue || decision.WasExtortedFrom.Value))
            {
                if (decision.GoOnHeist
                    && !heistHappens)
                {
                    fateTitle = fateTitle + "ARRESTED FOR FALSE REPORT. ";
                    fateMessage = fateMessage + FalseReportNoHeistYouGo;
                }
                else if (decision.GoOnHeist
                    && heistHappens)
                {
                    fateTitle = fateTitle + "GOT YOURSELF ARRESTED. ";
                    fateMessage = fateMessage + GetYourselfArrested;
                }
                else if (!decision.GoOnHeist
                    && heistHappens)
                {
                    fateTitle = fateTitle + "SNITCH SUCCESS. ";
                    fateMessage = fateMessage + SnitchSuccess(decision);
                }
                else if (!decision.GoOnHeist
                    && !heistHappens)
                {
                    fateTitle = fateTitle + "ARRESTED FOR FALSE REPORT. ";
                    fateMessage = fateMessage + FalseReportNoHeistNoGo;
                }
            }

            // Resolve blackmailing others who went on the heist
            if (decision.PlayerToBlackmail != null
                && decision.PlayerToBlackmail.Decision.GoOnHeist)
            {
                if (decision.ExtortionSuccessful.HasValue
                    && decision.ExtortionSuccessful.Value)
                {
                    if (heistHappens
                        && policeReported)
                    {
                        fateTitle = fateTitle + $"BLACKMAILED {decision.PlayerToBlackmail.Name}. ";
                        fateMessage = fateMessage + BlackmailSuccessAfterHeistArrest(decision);
                    }
                    else if (heistHappens
                        && !policeReported)
                    {
                        fateTitle = fateTitle + $"BLACKMAILED {decision.PlayerToBlackmail.Name}. ";
                        fateMessage = fateMessage + BlackmailSuccessAfterHeistSuccess(decision);
                    }
                    else if (!heistHappens)
                    {
                        fateTitle = fateTitle + $"BLACKMAILED {decision.PlayerToBlackmail.Name}. ";
                        fateMessage = fateMessage + BlackmailSuccessAfterHeistDisband(decision);
                    }
                }
                else if (decision.ExtortionSuccessful.HasValue
                    && !decision.ExtortionSuccessful.Value)
                {
                    if (heistHappens
                        && policeReported)
                    {
                        fateTitle = fateTitle + $"FAILED TO BLACKMAIL {decision.PlayerToBlackmail.Name} . ";
                        fateMessage = fateMessage + BlackmailFailureAfterHeistArrest(decision);
                    }
                    else if (heistHappens
                        && !policeReported)
                    {
                        fateTitle = fateTitle + $"FAILED TO BLACKMAIL {decision.PlayerToBlackmail.Name} AND ARRESTED. ";
                        fateMessage = fateMessage + BlackmailFailureAfterHeistSuccess(decision);
                    }
                    else if (!heistHappens)
                    {
                        fateTitle = fateTitle + $"FAILED TO BLACKMAIL {decision.PlayerToBlackmail.Name} AND ARRESTED. ";
                        fateMessage = fateMessage + BlackmailFailureAfterHeistDisband(decision);
                    }
                }
            }

            // Resolve getting blackmailed and going on a heist
            if (decision.Blackmailers != null
                && decision.GoOnHeist)
            {
                if (decision.WasExtortedFrom.HasValue
                    && decision.WasExtortedFrom.Value)
                {
                    if (heistHappens
                        && policeReported)
                    {
                        fateTitle = fateTitle + "EXPOSED AS A SNITCH. ";
                        fateMessage = fateMessage + BlackmailedAfterArrest(decision);
                    }
                    else if (heistHappens
                        && !policeReported)
                    {
                        fateTitle = fateTitle + "EXPOSED AS A SNITCH. ";
                        fateMessage = fateMessage + BlackmailedAfterSuccessfulHeist(decision);
                    }
                    else if (!heistHappens)
                    {
                        fateTitle = fateTitle + "EXPOSED AS A SNITCH. ";
                        fateMessage = fateMessage + BlackmailedAfterDisbanding(decision);
                    }
                }
                else if (decision.WasExtortedFrom.HasValue
                    && !decision.WasExtortedFrom.Value)
                {
                    if (heistHappens
                        && policeReported)
                    {
                        fateTitle = fateTitle + "FALSELY ACCUSED. ";
                        fateMessage = fateMessage + DefendedSelfAfterArrest(decision);
                    }
                    else if (heistHappens
                        && !policeReported)
                    {
                        fateTitle = fateTitle + "FALSELY ACCUSED. ";
                        fateMessage = fateMessage + DefendedSelfAfterSuccessfulHeist(decision);
                    }
                    else if (!heistHappens)
                    {
                        fateTitle = fateTitle + "FALSELY ACCUSED. ";
                        fateMessage = fateMessage + DefendedSelfAfterDisbanding(decision);
                    }
                }

                if (decision.JailTerm > 0)
                {
                    fateMessage = fateMessage + SentencedToJail(decision);
                }
            }

            return Tuple.Create(fateTitle, fateMessage);
        }

        public static Tuple<string, string> DecisionMessage(Player.HeistDecision decision)
        {
            string decisionTitle = "";
            string decisionDescription = "";
            if (decision.PlayerToBlackmail != null)
            {
                decisionTitle = "COMMIT BLACKMAIL";
                decisionDescription = string.Format("You have decided to blackmail {0} if the opportunity presents itself while on this heist.", decision.PlayerToBlackmail.Name);
            }
            else if (decision.GoOnHeist && !decision.ReportPolice)
            {
                decisionTitle = "GO ON HEIST";
                decisionDescription = "You decide to go on the heist.";
            }
            else if (!decision.GoOnHeist && !decision.ReportPolice)
            {
                decisionTitle = "RUN AWAY";
                decisionDescription = "You have better things to do than risk your life on this. You stay far away.";
            }
            else if (decision.GoOnHeist && decision.ReportPolice)
            {
                decisionTitle = "GET YOURSELF ARRESTED";
                decisionDescription = "You look at the bunch of criminals around you and figure you're screwed anyway - might as well be a snitch and get some cash. But you want to spend some time in jail to avoid suspicion.";
            }
            else if (!decision.GoOnHeist && decision.ReportPolice)
            {
                decisionTitle = "SNITCH";
                decisionDescription = "You decide to tell the police that there's a heist going on. You'll watch your fellow thieves from close by and keep the police informed.";
            }

            return Tuple.Create(decisionTitle, decisionDescription);
        }

        public static Tuple<string, string> GetBlackmailedMessage(Player.HeistDecision decision)
        {
            var decisionTitle = "BLACKMAILED.";
            var decisionMessage = $"While snooping around, you ran into {GetPlayerNames(decision.Blackmailers)}." +
                $" Clearly, your correspondence with the police has been less clandestine than you wanted it to be. " +
                $"You hand over ${decision.Blackmailers[0].Decision.BlackmailReward} MILLION " +
                (decision.Blackmailers.Count > 1 ? " to each of them in exchange for their silence. " : " in exchange for this to never be spoken off again.");
            return Tuple.Create(decisionTitle, decisionMessage);
        }

        public static Tuple<string, string> Bailing
        {
            get
            {
                return Tuple.Create("TAKING A VACATION!", "You spend the year doing other fun exciting things. Much like crime, but without the downside of getting arrested.");
            }
        }

        public static Tuple<string, string> BlackmaileeMissingMessage(Player.HeistDecision decision)
        {
            var decisionTitle = $"COULD NOT FIND {decision.PlayerToBlackmail.Name}.";
            var decisionMessage = $"Your contacts searched all over for {decision.PlayerToBlackmail.Name} but couldn't find him, which ruins your plans for blackmail.";
            return Tuple.Create(decisionTitle, decisionMessage);
        }

        public static Tuple<string, string> BlackmailSnitchMessage(Player.HeistDecision decision)
        {
            var decisionTitle = $"FOUND {decision.PlayerToBlackmail.Name} SNITCHING.";
            var decisionMessage = $"You conveniently found {decision.PlayerToBlackmail.Name} lurking in the area, about to tell the police about your crimes. " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"Unsurprisingly, you find that {decision.PlayerToBlackmail.Decision.Blackmailers.Count - 1} of the thieves have the same idea." : "") +
                $"The snitch is forced to hand over ${decision.BlackmailReward} MILLION " +
                (decision.Blackmailers.Count > 1 ? " to each thief in exchange for their silence. " : " to you in exchange for this to never be spoken off again.");
            return Tuple.Create(decisionTitle, decisionMessage);
        }

        public static string HeistAbandoned(List<Player> players)
        {
            return $"Thieves who showed up for heist: {GetPlayerNames(players)}. After much deliberation, you decide it's far too risky, and call the whole thing off. This is far too big a job for just {players.Count} people. ";
        }

        public static string HeistArrested
        {
            get
            {
                return $"You knew it seemed too good to be true. As you were about the step into the getaway car, the police came out of nowhere. Someone has been informing on your whereabouts. ";
            }
        }

        public static string SentencedToJail(Player.HeistDecision decision)
        {
            return $"You are sentenced to {decision.JailTerm} years in jail. Without parole. " +
                (decision.JailFine > 0 ? $"Your lawyer fees and fines also incur you a loss of ${decision.JailFine} MILLION. " : "");
        }

        public static string HeistSuccess(Player.HeistDecision decision)
        {
            return $"{GetPlayerNames(decision.FellowHeisters)} successfully {decision.HeistSuccessMessage}! " +
                $"At the end of the day, you earn ${decision.HeistReward} MILLION for your troubles. ";
        }

        public static string FalseReportNoHeistYouGo
        {
            get
            {
                return "You inform the police that the heist is happening. And then the heist doesn't happen. " +
                    "No matter how hard you convince others to go. " +
                    "Unfortunately, that turns the police's attention towards you. " +
                    "Some of your shady past bubbles back up, and you're arrested. ";
            }
        }

        public static string GetYourselfArrested
        {
            get
            {
                return "You ratted on your team's whereabouts to the police. They thought they were safe, when the police barged in and arrested them. " +
                    "Unfortunately, it seems that the police has also found out about your shady past and also arrested you. At least you might be able to get a reduced sentence. ";
            }
        }

        public static string SnitchSuccess(Player.HeistDecision decision)
        {
            return $"You watch from afar as the heist takes place, informing the police about what is happening. " +
                $"When they least expect it, the thieves are snatched off to jail. And you are rewarded with ${decision.HeistReward} MILLION. ";
        }

        public static string FalseReportNoHeistNoGo
        {
            get
            {
                return "You inform the police that the heist is happening. And then the heist doesn't happen. " +
                    "Unfortunately, that turns the police's attention towards you. " +
                    "Some of your shady past bubbles back up, and you're arrested. ";
            }
        }

        public static string BlackmailSuccessAfterHeistArrest(Player.HeistDecision decision)
        {
            return $"While in lockup, you confront {decision.PlayerToBlackmail.Name} about his previous dealings with the police. " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"Unsurprisingly, you find that {decision.PlayerToBlackmail.Decision.Blackmailers.Count - 1} of the thieves have the same idea." : "") +
                $"You reckon prison won't treat a snitch very well. You get ${decision.BlackmailReward} MILLION in exchange for your silence. ";
        }

        public static string BlackmailSuccessAfterHeistSuccess(Player.HeistDecision decision)
        {
            return $"On the way back from the heist, you confront {decision.PlayerToBlackmail.Name} about his previous dealings with the police. " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"Unsurprisingly, you find that {decision.PlayerToBlackmail.Decision.Blackmailers.Count - 1} of the thieves have the same idea." : "") +
                $"You get ${decision.BlackmailReward} MILLION in exchange for your silence. ";
        }

        public static string BlackmailSuccessAfterHeistDisband(Player.HeistDecision decision)
        {
            return $"As you all decide to go your separate ways, you confront {decision.PlayerToBlackmail.Name} about his previous dealings with the police. " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"Unsurprisingly, you find that {decision.PlayerToBlackmail.Decision.Blackmailers.Count - 1} of the thieves have the same idea." : "") +
                $"You get ${decision.BlackmailReward} MILLION in exchange for your silence. ";
        }

        public static string BlackmailFailureAfterHeistArrest(Player.HeistDecision decision)
        {
            return $"While in lockup, you interrogate {decision.PlayerToBlackmail.Name} about his past, but learn nothing. " +
                $"Unfortunately, {decision.PlayerToBlackmail.Name} does not appreciate your nosiness and his lawyers do a number on you, extending your prison sentence. ";
        }

        public static string BlackmailFailureAfterHeistSuccess(Player.HeistDecision decision)
        {
            return $"On the way back from the heist, you interrogate {decision.PlayerToBlackmail.Name} about whether he has had dealings with the police. " +
                $"Unfortunately, your accusations don't seem to be appreciate. " +
                $"The next day, you find out that someone has told the police to find out more about your criminal record. ";
        }

        public static string BlackmailFailureAfterHeistDisband(Player.HeistDecision decision)
        {
            return $"As you all decide to go your separate ways, you accuse {decision.PlayerToBlackmail.Name} of having dealings with the police. " +
                $"Unfortunately, your accusations don't seem to be appreciate. " +
                $"The next day, you find out that someone has told the police to find out more about your criminal record. ";
        }

        public static string BlackmailedAfterArrest(Player.HeistDecision decision)
        {
            return $"While in lockup, you are confronted by {GetPlayerNames(decision.Blackmailers)} about your dealings with the police. " +
                $"They seem to have unrefutable evidence, and you reckon prison won't treat a snitch very well. " +
                $"You have no choice but to cough up ${decision.Blackmailers[0].Decision.BlackmailReward} MILLION " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"to each of them for their silence. " : $"in exchange for silence. ");
        }

        public static string BlackmailedAfterSuccessfulHeist(Player.HeistDecision decision)
        {
            return $"On your way back from the heist, you're stopped by by {GetPlayerNames(decision.Blackmailers)}. " +
                $"Irrefutable evidence of your dealings with the police have gotten out. " +
                $"You have no choice but to cough up ${decision.Blackmailers[0].Decision.BlackmailReward} MILLION " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"to each of them for their silence, and to let you live. " : $"in exchange for silence, and to let you live. ");
        }

        public static string BlackmailedAfterDisbanding(Player.HeistDecision decision)
        {
            return $"Just after you step away from the meeting, you're stopped by by {GetPlayerNames(decision.Blackmailers)}. " +
                $"Irrefutable evidence of your dealings with the police have gotten out. " +
                $"You have no choice but to cough up ${decision.Blackmailers[0].Decision.BlackmailReward} MILLION " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"to each of them for their silence, and to let you live. " : $"in exchange for silence, and to let you live. ");
        }

        public static string DefendedSelfAfterArrest(Player.HeistDecision decision)
        {
            return $"While in lockup, you are accused by {GetPlayerNames(decision.Blackmailers)} of being a snitch. " +
                $"However, there is no concrete evidence against you, and you don't appreciate being antagonized. " +
                $"After a lengthy talk with your lawyers, you manage to " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"pin more crimes on them, extending their jail sentences. " : $"find ways to get {decision.Blackmailers[0].Name} a longer jail sentence. ");
        }

        public static string DefendedSelfAfterSuccessfulHeist(Player.HeistDecision decision)
        {
            return $"On your way back from the heist, you're stopped by by {GetPlayerNames(decision.Blackmailers)} and accused of being a snitch. " +
                $"However, there is no concrete evidence against you, and you don't appreciate being antagonized. " +
                $"You devote a good portion of the next year to " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"unearthing evidence of their previous crimes and landing them in jail. " : $"unearthing evidence of {decision.Blackmailers[0].Name}'s previous crimes and landing {decision.Blackmailers[0].Name} in jail. ");
        }

        public static string DefendedSelfAfterDisbanding(Player.HeistDecision decision)
        {
            return $"Just after you step away from the meeting, you're stopped by {GetPlayerNames(decision.Blackmailers)} and accused of being a snitch. " +
                $"However, there is no concrete evidence against you, and you don't appreciate being antagonized. " +
                $"You devote a good portion of the next year to " +
                (decision.PlayerToBlackmail.Decision.Blackmailers.Count > 1 ? $"unearthing evidence of their previous crimes and landing them in jail. " : $"unearthing evidence of {decision.Blackmailers[0].Name}'s previous crimes and landing {decision.Blackmailers[0].Name} in jail. ");
        }

        private static string GetPlayerNames(List<Player> players)
        {
            if (players == null || players.Count == 0)
            {
                return string.Empty;
            }

            if (players.Count == 1)
            {
                return players[0].Name;
            }

            if (players.Count == 2)
            {
                return players[0].Name + " and " + players[1].Name;
            }

            var playerNames = new StringBuilder();
            for (var i = 0; i < playerNames.Length-1; i++)
            {
                playerNames.Append(players[i].Name + ", ");
            }

            playerNames.Append("and ");
            playerNames.Append(players[playerNames.Length - 1].Name);
            return playerNames.ToString();
        }
    }
}
