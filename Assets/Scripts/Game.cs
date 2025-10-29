using System.Runtime.CompilerServices;

namespace TexasHoldem
{
    public static class Game
    {
        // static members
        // fields
        static State state = State.Start;
        static int round = 1;
        static int dealer = 0;
        static int smallBlind = 0;
        static int bigBlind = 0;
        static int pot = 0;

        // constants
        public const int BOARD_SIZE = 5;
        public const int HAND_SIZE = BOARD_SIZE + Hole.SIZE;
        public const int SMALL_BLIND = 10;
        public const int BIG_BLIND = 25;

        // properties
        public static float InitialProjectionSize { get; private set; }
        public static float FocusProjectionSize { get; } = 2.5f;
        public static State State
        {
            get { return state; }
            private set
            { 
                state = value >= State.Start && value < State.NextRound ?
                    value : state = State.Start;
            }
        }
        public static int Round
        {
            get { return round; }
            private set { round = value < int.MaxValue ? value : 2; }
        }
        public static bool IsDealerDetermined { get; set; } = false;
        public static bool HasRoundStarted { get; set; } = false;
        public static int Pot
        {
            get { return pot; }
            private set { pot = value; }
        }
        public static int Dealer
        {
            get { return dealer; }
            private set { dealer = value < Player.Count ? value : 0; }
        }
        public static int SmallBlind
        {
            get { return smallBlind; }
            private set { smallBlind = value < Player.Count ? value : 0; }
        }
        public static int BigBlind
        {
            get { return bigBlind; }
            private set { bigBlind = value < Player.Count ? value : 0; }
        }

        // methods
        /// <summary>
        /// sets the inizial projection size of the main camera
        /// </summary>
        /// <param name="size"></param>
        /// <exception cref="System.InvalidOperationException"></exception>
        public static void SetInitialProjectionSize(float size)
        {
            if (InitialProjectionSize != 0f)
                throw new System.InvalidOperationException("This method can only be called once.");

            InitialProjectionSize = size;
        }
        /// <summary>
        /// increments state
        /// </summary>
        public static void NextState() { State++; }

        /// <summary>
        /// decrements state
        /// </summary>
        public static void PreviousState() { State--; }

        /// <summary>
        /// increments round
        /// </summary>
        public static void NextRound() { Round++; }

        /// <summary>
        /// adds the sum of bets to the pot
        /// </summary>
        /// <param name="betSum"></param>
        public static void AddToPot(int betSum) { Pot += betSum; }

        /// <summary>
        /// clears the pot during payout
        /// </summary>
        public static void ClearPot() { Pot = 0; }

        /// <summary>
        /// sets the initial dealer, should only be called on the first round
        /// </summary>
        /// <param name="dealerIndex"></param>
        public static void SetInitialDealer(int dealerIndex) { Dealer = dealerIndex; }

        /// <summary>
        /// increments DealerIndex
        /// </summary>
        public static void NextDealer() { Dealer++; }

        /// <summary>
        /// set blinds
        /// </summary>
        public static void SetBlinds()
        {
            SmallBlind = Player.Count > 2 ? Dealer + 1 : Dealer;
            BigBlind = SmallBlind + 1;
        }
    }

    // enums
    /// <summary>
    /// game states
    /// </summary>
    public enum State
    {
        Start,
        RoundStart,
        Deal,
        Play,
        Flip,
        NextRound
    }
}
