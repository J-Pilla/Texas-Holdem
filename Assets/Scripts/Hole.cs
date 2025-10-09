using static GameManager;

public class Hole
{
    // constant fields
    public const int SIZE = 2;

    // non-static properties
    public Card[] Cards { get; private set; }
    public int CardCount { get; private set; }
    public Rank HighCard { get; set; }
    public Rank Kicker { get; set; }

    // non-static methods
    public void AddCard(int cardId)
    {
        if (CardCount < SIZE)
        {
            Cards[CardCount] = new Card(cardId, true);
            CardCount++;
        }
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
