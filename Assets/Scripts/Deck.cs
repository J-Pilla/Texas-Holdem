using System;

public class Deck
{
    // static members
    public const byte SIZE = 52;
    public const byte SHUFFLE_COUNT = 4;

    // non-static members & properties
    int[] cardIds;
    public int[] CardIds { get { return cardIds; } }

    // non-static methods
    /// <summary>
    /// randomize the order of the card ids in the deck
    /// and sets deckIndex back down to 0
    /// </summary>
    public void Shuffle()
    {
        Random randomNumber = new Random();

        for (int ctr = 0; ctr < SHUFFLE_COUNT; ctr++)
        {
            if (ctr == SHUFFLE_COUNT - 1)
                Cut();

            for (int index = 0; index < SIZE; index++)
            {
                int randomIndex = randomNumber.Next(index, SIZE - 1);
                int tempId = cardIds[index];
                cardIds[index] = cardIds[randomIndex];
                cardIds[randomIndex] = tempId;
            }
        }
    }

    void Cut()
    {
        Random randomNumber = new Random();
        int cutPoint = randomNumber.Next(22, 29);
        int remainingCards = SIZE - cutPoint;
        int[] lowerIds = new int[cutPoint];

        for (int index = 0; index < cutPoint; index++)
            lowerIds[index] = cardIds[index];

        for (int index = 0; index < SIZE; index++)
        {
            if (index < remainingCards)
                cardIds[index] = cardIds[index + cutPoint];
            else
                cardIds[index] = lowerIds[index - remainingCards];
        }
    }

    // constructors
    public Deck()
    {
        cardIds = new int[SIZE];
        InitializeDeck();
    }

    /// <summary>
    /// initializes the deck so each element carries a card id equal to the index
    /// </summary>
    void InitializeDeck()
    {
        for (int index = 0; index < SIZE; index++)
            cardIds[index] = index;
    }
}
