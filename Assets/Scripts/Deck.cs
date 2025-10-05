using System;

public static class Deck
{
    // members & properties
    public const byte SIZE = 52;
    public const byte SHUFFLE_COUNT = 4;

    public static int[] CardIds { get; private set; } = new int[SIZE];
    public static int CardIndex { get; set; } = 0;

    // methods
    /// <summary>
    /// initializes the deck so each element carries a card id equal to the index
    /// </summary>
    public static void InitializeDeck()
    {
        for (int index = 0; index < SIZE; index++)
            CardIds[index] = index;
    }
    /// <summary>
    /// randomize the order of the card ids in the deck
    /// and sets deckIndex back down to 0
    /// </summary>
    public static void Shuffle()
    {
        Random randomNumber = new Random();

        CardIndex = 0;

        for (int ctr = 0; ctr < SHUFFLE_COUNT; ctr++)
        {
            if (ctr == SHUFFLE_COUNT - 1)
                Cut();

            for (int index = 0; index < SIZE; index++)
            {
                int randomIndex = randomNumber.Next(index, SIZE - 1);
                int tempId = CardIds[index];
                CardIds[index] = CardIds[randomIndex];
                CardIds[randomIndex] = tempId;
            }
        }
    }

    static void Cut()
    {
        Random randomNumber = new Random();
        int cutPoint = randomNumber.Next(22, 29);
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
}
