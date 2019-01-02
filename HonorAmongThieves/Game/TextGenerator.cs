using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public static class TextGenerator
    {
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

        public static Tuple<string, string> StillDead
        {
            get
            {
                return Tuple.Create("STILL DEAD", "Unfortunately, death seems to be a very difficult to reverse state.");
            }
        }

        public static Tuple<string, string> DeathMessage(Player player)
        {
            var fateTitle = "DEAD ";
            var fateDescription = "You got killed! ";
            player.Decision.FateTitle = player.Decision.FateTitle + "DEAD ";
            player.Decision.FateDescription = player.Decision.FateDescription + "You got killed! ";

            if (player.Decision.PlayerToKill != null
                && player.Decision.Killers.Count == 1
                && player.Decision.Killers.Contains(player))
            {
                fateDescription = fateDescription + string.Format(DeathByFalseAccusation, player.Decision.PlayerToKill.Name);
            }

            else if (player.Decision.GoOnHeist)
            {
                fateDescription = fateDescription + DeathByHeistmember;

            }
            else
            {
                fateDescription = fateDescription + DeathBySneakingAround;
            }

            return Tuple.Create(fateTitle, fateDescription);
        }

        private static string DeathByFalseAccusation = "You confronted {0}. Things got heated, and someone lost their head. Unfortunately, that someone was you. ";

        private static string DeathByHeistmember = "You were confronted at the heist. Things got heated, and the evidence against you piled up. As you ran away, you went down with an unfortunate case of a bullet in the brain.";

        private static string DeathBySneakingAround = "You snuck around to snitch on the ongoing heist, and noticed that something wasn't right. But too late. Someone snuck behind you and lobbed off your head.";

        public static Tuple<string, string> DefenseMessage(Player player)
        {
            var fateTitle = "DEFENDED YOURSELF ";
            string fateDescription;
            if (player.Room.SnitchMurderWindow >= 0)
            {
                fateDescription = DefenseAndRetaliationJailing;
            }
            else
            {
                fateDescription = DefenseAndRetaliationKilling;
            }

            return Tuple.Create(fateTitle, fateDescription);
        }

        private static string DefenseAndRetaliationJailing = "You were accused of being a snitch. But your friends came to your aid. They left the accuser lying on the floor for the police to deal with. ";

        private static string DefenseAndRetaliationKilling = "You were accused of being a snitch. But your friends came to your aid, leaving your accuser dead. ";
    }
}
