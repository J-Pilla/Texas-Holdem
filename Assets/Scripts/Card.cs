using System.Text.RegularExpressions;

namespace TexasHoldem
{
    /// <summary>
    /// class representing a card, uses CardIds
    /// from Deck.cs to initialize unique cards
    /// </summary>
    public class Card
    {
        // fields
        int id;

        // properties
        public int Id
        {
            get { return id; }
            set
            {
                if (value >= 0 && value < Deck.SIZE)
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
                return $"card-{Suit}-{(int)(Rank == Rank.Ace ? Rank.LowAce : Rank)}".ToLower();
            }
        }

        // methods
        public override string ToString()
        {
            return $"Card: {Name}, Id: {Id}, Player Card: {InPlayerHand}";
        }

        // constructors
        public Card(int id, bool inPlayerHand = false)
        {
            Id = id;
            Rank = (Rank)(Id % 13 + 2);
            Suit = (Suit)(Id % 4);
            InPlayerHand = inPlayerHand;
        }

        /// <summary>
        /// used to force cards for testing purposes
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="suit"></param>
        /// <param name="inPlayerHand"></param>
        [System.Obsolete]
        public Card(Rank rank, Suit suit, bool inPlayerHand)
        {
            Id += ((int)rank - 2) * ((int)suit + 1);
            Rank = rank;
            Suit = suit;
            InPlayerHand = inPlayerHand;
        }
    }

    /// <summary>
    /// card ranks
    /// </summary>
    public enum Rank
    {
        LowAce = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    /// <summary>
    /// entensions for Rank enum
    /// </summary>
    public static class RankExtensions
    {
        public static string GetName(this Hand hand)
        {
            string name = hand.ToString();

            switch (hand)
            {
                case Hand.NoPair:
                case Hand.OnePair:
                case Hand.TwoPair:
                case Hand.FullHouse:
                case Hand.StraightFlush:
                case Hand.RoyalFlush:
                    string pattern = @"([a-z])([A-Z])";
                    string replacement = "$1 $2";
                    name = Regex.Replace(name, pattern, replacement);
                    break;
                case Hand.ThreeOAK:
                case Hand.FourOAK:
                    name = name.Replace("OAK", " of a Kind");
                    break;
            }

            return name;
        }
    }

    /// <summary>
    /// card suits
    /// </summary>
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }
}
