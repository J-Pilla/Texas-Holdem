using System.Text.RegularExpressions;

namespace TexasHoldem
{
    public static class Player
    {
        // fields
        static int dealerIndex = 0;
        static int smallBlindIndex = 0;
        static int bigBlindIndex = 0;

        // constants
        public const int MAX = 10;
        public const int MIN = 2;

        // properties
        public static int Count { get; private set; } = 0;
        public static int DealerIndex
        {
            get { return dealerIndex; }
            private set { dealerIndex = value < Count ? value : 0; }
        }
        public static int SmallBlindIndex
        {
            get { return smallBlindIndex; }
            private set { smallBlindIndex = value < Count ? value : 0; }
        }
        public static int BigBlindIndex
        {
            get { return bigBlindIndex; }
            private set { bigBlindIndex = value < Count ? value : 0; }
        }

        // methods
        /// <summary>
        /// increments Count
        /// </summary>
        public static void IncrementCount() { Count++; }

        /// <summary>
        /// decrements Count
        /// </summary>
        public static void DecrementCount() { Count--; }

        /// <summary>
        /// sets the initial dealer, should only be called on the first round
        /// </summary>
        /// <param name="dealerIndex"></param>
        public static void SetInitialDealer(int dealerIndex) { DealerIndex = dealerIndex; }

        /// <summary>
        /// increments DealerIndex
        /// </summary>
        public static void NextDealer()
        {
            DealerIndex++;
            SetBlinds();
        }

        /// <summary>
        /// set blinds
        /// </summary>
        public static void SetBlinds()
        {
            SmallBlindIndex = Count > 2 ? DealerIndex + 1 : DealerIndex;
            BigBlindIndex = SmallBlindIndex + 1;
        }
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
        Dealer = -1,
        None,
        Small,
        Big
    }
}
