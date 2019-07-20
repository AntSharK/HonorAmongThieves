using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Cakery.GameLogic
{
    public class CakeryPlayer : Player
    {
        public enum Status
        {
            WaitingForGameStart,
            Producing,
            MarketReport,
            CleaningUp
        }

        public class Resources
        {
            public double Butter = 0;
            public double Flour = 0;
            public double Sugar = 0;
            public double Money = 0;
        }

        public class BakedGoods
        {
            public long Cookies = 0;
            public (double butter, double flour, double sugar, double money) CookieCost =
                (100d, 100d, 100d, 100d);
            public long Croissants = 0;
            public (double butter, double flour, double sugar, double money) CroissantCost =
                (500d, 500d, 500d, 100d);
            public long Cakes = 0;
            public (double butter, double flour, double sugar, double money) CakeCost =
                (2500d, 2500d, 2500d, 1500d);
        }

        public class Upgrades
        {
            // TODO
        }

        public CakeryRoom Room { get; set; }

        public Status CurrentStatus { get; set; } = Status.WaitingForGameStart;

        private Resources resources = new Resources();
        private BakedGoods bakedGoods = new BakedGoods();
        private Upgrades upgrades = new Upgrades();

        public CakeryPlayer(string playerName, CakeryRoom room)
            : base(playerName)
        {
            this.Room = room;
        }
    }
}
