using static GameManager;

public class Card
{
    // non-static members
    int id;

    // non-static properties
    public int Id
    {
        get { return id; }
        set
        {
            if (value >= 0)
                id = value;
            else
                id = 0;
        }
    }
    public Rank Rank { get; }
    public Suit Suit { get; }
    public bool InPlayerHand { get; }
    public string Name { get { return $"{Rank} of {Suit}"; } }
    public string File
    {
        get
        {
            return ($"card-{Suit}-{(int)(Rank == Rank.Ace ? Rank.LowAce: Rank)}").ToLower();
        }
    }

    // constructors
    public Card(int id, bool inPlayerHand = false)
    {
        Id = id;
        Rank = (Rank)(Id % 13 + 2);
        Suit = (Suit)(Id % 4);
        InPlayerHand = inPlayerHand;
    }
     public Card (Rank rank, Suit suit, bool inPlayerHand)
    {
        Id += (int)suit;
        Rank = rank;
        Suit = suit;
        InPlayerHand = inPlayerHand;
    }
}
