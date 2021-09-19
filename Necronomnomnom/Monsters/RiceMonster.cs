using System;
using System.Collections.Generic;
using System.Text;
using Necronomnomnom.Cards;

namespace Necronomnomnom.Monsters
{
    /// <summary>
    /// This monster will rice from the dead
    /// It absorbs everything
    /// </summary>
    public class RiceMonster : Monster
    {
        public override int MaxHitPoints => 10;
        
        public RiceMonster()
        {
            this.HitPoints = 10;
            this.Cards.Add(new Nullify());
        }
    }
}
