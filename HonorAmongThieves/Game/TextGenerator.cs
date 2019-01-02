using System;

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
    }
}
