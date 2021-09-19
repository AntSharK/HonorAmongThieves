using System;
using System.Collections.Generic;

namespace Necronomnomnom
{
    /// <summary>
    /// Represents all modifiers in a round which can be applied onto a card
    /// </summary>
    public class CardModifierState
    {
        private int dmgMult = 1;
        private int dmgInc = 0;
        private int durMult = 1;
        private int durInc = 0;

        public List<Action<CardModifierState, int>> OnDmgMultChange = new List<Action<CardModifierState, int>>();
        public List<Action<CardModifierState, int>> OnDmgIncChange = new List<Action<CardModifierState, int>>();
        public List<Action<CardModifierState, int>> OnDurMultChange = new List<Action<CardModifierState, int>>();
        public List<Action<CardModifierState, int>> OnDurIncChange = new List<Action<CardModifierState, int>>();

        public int DamageMultiplier { get { return this.dmgMult; } 
            set {
                var oldVal = this.dmgMult;
                this.dmgMult = value;
                foreach (var a in this.OnDmgMultChange) { a(this, oldVal); }
            } }

        public int DamageIncrease { get { return this.dmgInc; }
            set {
                var oldVal = this.dmgInc;
                this.dmgInc = value;
                foreach (var a in this.OnDmgIncChange) { a(this, oldVal); }
            } }

        public int DurationMultipler { get { return this.durMult; }
            set {
                var oldVal = this.durMult;
                this.durMult = value;
                foreach (var a in this.OnDurMultChange) { a(this, oldVal); }
            } }

        public int DurationIncrease { get { return this.durInc; }
            set {
                var oldVal = this.durInc;
                this.durInc = value;
                foreach (var a in this.OnDurIncChange) { a(this, oldVal); }
            } }

        public int GetDuration(int baseDuration)
        {
            return (baseDuration + this.DurationIncrease) * this.DurationMultipler;
        }

        public int GetDamageDone(int baseDamage)
        {
            return (baseDamage + this.DamageIncrease) * this.DamageMultiplier;
        }
    }
}
