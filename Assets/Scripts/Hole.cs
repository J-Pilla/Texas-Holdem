using static GameManager;

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
    /// Adds a Card to the Cards array
    /// </summary>
    /// <param name="cardId"></param>
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
