using UnityEngine;

namespace TexasHoldem
{
    /// <summary>
    /// class representing a "hole" in Texas Holdem,
    /// the two cards each player is dealt
    /// </summary>
    public class Hole
    {
        // static members
        // constants
        public const int SIZE = 2;

        // non-static members
        // properties
        public Card[] Cards { get; private set; }
        public int CardCount { get; private set; }
        public Rank HighCard { get; set; }
        public Rank Kicker { get; set; }

        // methods
        public override string ToString()
        {
            return $"Hole: {CardCount} cards, High Card: {HighCard}, Kicker {Kicker}";
        }

        /// <summary>
        /// adds a Card to the Cards array
        /// </summary>
        /// <param name="cardId"></param>
        public void AddCard(int cardId, GameObject cardPrefab, Transform parent)
        {
            if (CardCount < SIZE)
            {
                Cards[CardCount] = new Card(cardId, true, cardPrefab, parent);
                Cards[CardCount].OffSetPlayerCard(CardCount);
                CardCount++;
            }
        }

        public void FlipCards()
        {
            for (int index = 0; index < CardCount; index++)
                Cards = new Card[SIZE];
        }

        /// <summary>
        /// removes Cards from Cards array
        /// </summary>
        public void Discard()
        {
            for (int index = 0; index < CardCount; index++)
                Cards[index].Discard();
            
            Cards = new Card[SIZE];
            CardCount = 0;
        }

        // constructors
        public Hole()
        {
            Cards = new Card[SIZE];
            CardCount = 0;
            HighCard = Rank.LowAce;
            Kicker = Rank.LowAce;
        }
    }
}
