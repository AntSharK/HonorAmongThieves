using System;
using System.Collections.Generic;

namespace Necronomnomnom
{
    /// <summary>
    /// Represents a regular player
    /// </summary>
    public class Player
    {
        private static Random rng = new Random();
        private int hitPoints = 100;
        public List<Card> Cards = new List<Card>();
        public List<Card> CurrentHand = new List<Card>();

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
            if (this.MaxHandSize >= this.Cards.Count)
            {
                this.CurrentHand = this.Cards;
                return;
            }

            // Take random cards from the deck and put it into the current hand
            this.CurrentHand = new List<Card>();
            var indexes = new List<int>();
            for(var i = 0; i < this.Cards.Count; i++)
            {
                indexes.Add(i);
            }

            for(var i = 0; i < this.MaxHandSize; i++)
            {
                var randomIndex = rng.Next(0, indexes.Count);
                var index = indexes[randomIndex]; // Pick a random number from the index
                indexes.RemoveAt(randomIndex);
                this.CurrentHand.Add(this.Cards[index]);
            }
        }

        public void GiveCard(Card card)
        {
            card.Owner = this;
            this.Cards.Add(card);
        }
    }
}
