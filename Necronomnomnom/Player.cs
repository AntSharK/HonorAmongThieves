using System.Collections.Generic;

namespace Necronomnomnom
{
    /// <summary>
    /// Represents a regular player
    /// </summary>
    public class Player
    {
        public List<Card> Cards = new List<Card>();

        public void GiveCard(Card card)
        {
            card.Owner = this;
            this.Cards.Add(card);
        }
    }
}
