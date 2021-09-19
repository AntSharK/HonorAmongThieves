using System;
using System.Collections.Generic;
using HonorAmongThieves;

namespace Necronomnomnom
{
    /// <summary>
    /// Represents a regular player
    /// </summary>
    public class Player
    {
        private int hitPoints = 100;
        public List<Card> Cards = new List<Card>();

        public int MaxHandSize = 2; // The number of cards the player can see at once
        public int MaxRefreshes = 2; // The number of times the player can refresh his hand
        public int CurrentRefreshes = 0;

        public int HitPoints
        {
            get { return this.hitPoints; }
            set
            {
                this.hitPoints = value;
                if (this.hitPoints > this.MaxHitPoints)
                {
                    this.hitPoints = this.MaxHitPoints;
                }
            }
        }
        public int MaxHitPoints { get; set; } = 100;

        public void NextRound()
        {
            this.RefreshHand();
            this.CurrentRefreshes = 0;
        }

        public void RefreshHand()
        {
            this.CurrentRefreshes++;
            Utils.Shuffle(this.Cards);
        }

        public void GiveCard(Card card)
        {
            card.Owner = this;
            this.Cards.Add(card);
        }
    }
}
