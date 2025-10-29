using System.Text.RegularExpressions;

namespace TexasHoldem
{
    public static class Player
    {
        // constants
        public const int MAX = 10;
        public const int MIN = 2;

        // properties
        public static int Count { get; private set; } = 0;

        // methods
        /// <summary>
        /// increments Count
        /// </summary>
        public static void IncrementCount() { Count++; }

        /// <summary>
        /// decrements Count
        /// </summary>
        public static void DecrementCount() { Count--; }
    }

    /// <summary>
    /// hand combinations
    /// </summary>
    public enum Hand
    {
        Fold,
        NoPair,
        OnePair,
        TwoPair,
        ThreeOAK,
        Straight,
        Flush,
        FullHouse,
        FourOAK,
        StraightFlush,
        RoyalFlush
    }

    /// <summary>
    /// entensions for Hand enum
    /// </summary>
    public static class HandExtensions
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
    /// blind state
    /// </summary>
    public enum Blind
    {
        None,
        Small,
        Big
    }
}
