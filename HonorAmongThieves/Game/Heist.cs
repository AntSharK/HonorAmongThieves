using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HonorAmongThieves.Game
{
    public class Heist
    {
        public string Id { get; private set; }

        public Dictionary<string, Player> Players { get; private set; } = new Dictionary<string, Player>();

        public Heist(string heistId)
        {
            this.Id = heistId;
        }
    }
}
