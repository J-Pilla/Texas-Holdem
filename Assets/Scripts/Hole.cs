using System.Text.RegularExpressions;
using static GameManager;

public class Hole
{
    // static members & properties
    public const byte SIZE = 2;

    // non-static members & properties
    Card[] cards;
    byte cardCount;
    public Card[] Cards { get { return cards; } }
    public byte CardCount { get { return cardCount; } }
    public Rank HighCard { get; set; }
    public Rank Kicker { get; set; }

    // non-static methods
    public void AddCard(int cardId)
    {
        cards[cardCount] = new Card(cardId, true);
        cardCount++;
    }

    // constructors
    public Hole()
    {
        cards = new Card[SIZE];
        cardCount = 0;
        HighCard = Rank.LowAce;
        Kicker = Rank.LowAce;
    }
}
