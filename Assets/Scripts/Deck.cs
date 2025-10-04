using System;

public class Deck
{
    // static members
    public const int SIZE = 52;

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

        for (int index = 0; index < SIZE; index++)
        {
            int randomIndex = randomNumber.Next(index, SIZE - 1);
            int tempId = cardIds[index];
            cardIds[index] = cardIds[randomIndex];
            cardIds[randomIndex] = tempId;
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
