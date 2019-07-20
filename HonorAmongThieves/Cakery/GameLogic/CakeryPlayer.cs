using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Cakery.GameLogic
{
    public class CakeryPlayer : Player
    {
        public CakeryRoom Room { get; set; }

        public CakeryPlayer(string playerName, CakeryRoom room)
            : base(playerName)
        {
            this.Room = room;
        }
    }
}
