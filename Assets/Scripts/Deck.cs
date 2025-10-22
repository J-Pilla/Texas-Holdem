using UnityEngine;

public static class Deck
{
    // constant fields
    public const int SIZE = 52;
    public const int SHUFFLE_COUNT = 4;

    // members
    static bool isInitialized = false;

    // properties
    public static int[] CardIds { get; private set; } = new int[SIZE];
    public static int CardIndex { get; private set; } = 0;

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
                int tempId = CardIds[index];
                CardIds[index] = CardIds[randomIndex];
                CardIds[randomIndex] = tempId;
            }
        }
    }
    /// <summary>
    /// initializes the deck so each element carries a card id equal to the index
    /// </summary>
    static void InitializeDeck()
    {
        for (int index = 0; index < SIZE; index++)
            CardIds[index] = (byte)index;
    }

    static void Cut()
    {
        int cutPoint = Random.Range(22, 29);
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

    public static void NextCard()
    {
        CardIndex++;
        if (CardIndex == SIZE)
            CardIndex = 0;
    }
}
