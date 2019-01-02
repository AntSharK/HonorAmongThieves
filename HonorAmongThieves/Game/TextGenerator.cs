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
    }
}
