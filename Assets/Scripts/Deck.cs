using UnityEngine;

namespace TexasHoldem
{
    /// <summary>
    /// static class representing a deck of cards,
    /// uses an array of integers to initialize unique Card objects
    /// </summary>
    public static class Deck
    {
        // fields
        static bool isInitialized = false;
        static int cardIndex = 0;

        // constants
        public const int SIZE = 52;
        public const int SHUFFLE_COUNT = 4;

        // properties
        public static int[] CardIds { get; private set; } = new int[SIZE];
        public static int CardIndex
        {
            get { return cardIndex; }
            private set { cardIndex = value < SIZE ? value : 0; }
        }

        // methods
        /// <summary>
        /// randomize the order of the card ids in the deck
        /// and sets deckIndex back down to 0
        /// </summary>
        public static void Shuffle()
        {
            CardIndex = 0;

            if (!isInitialized)
                InitializeDeck();

            for (int ctr = 0; ctr < SHUFFLE_COUNT; ctr++)
            {
                if (ctr == SHUFFLE_COUNT - 1)
                    Cut();

                for (int index = 0; index < SIZE; index++)
                {
                    int randomIndex = Random.Range(index, SIZE - 1);
                    (CardIds[index], CardIds[randomIndex]) = (CardIds[randomIndex], CardIds[index]);
                }
            }
        }

        /// <summary>
        /// initializes the deck so each element carries a card id equal to the index
        /// </summary>
        static void InitializeDeck()
        {
            isInitialized = true;

            for (int index = 0; index < SIZE; index++)
                CardIds[index] = (byte)index;
        }

        /// <summary>
        /// finds a random point close to the centre of the CardId array,
        /// then moves the ids after the point to the start of the array,
        /// bumping the ids before the point to the end of the array
        /// </summary>
        static void Cut()
        {
            int midPoint = SIZE / 2 - 1;
            int variance = 4;
            int cutPoint = Random.Range(midPoint - variance, midPoint + variance);
            int remainingCards = SIZE - cutPoint;
            int[] lowerIds = new int[cutPoint];

            for (int index = 0; index < cutPoint; index++)
                lowerIds[index] = CardIds[index];

            for (int index = 0; index < SIZE; index++)
            {
                if (index < remainingCards)
                    CardIds[index] = CardIds[index + cutPoint];
                else
                    CardIds[index] = lowerIds[index - remainingCards];
            }
        }

        /// <summary>
        /// increments CardIndex
        /// </summary>
        public static void NextCard() { CardIndex++; }
    }
}
