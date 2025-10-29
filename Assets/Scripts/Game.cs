namespace TexasHoldem
{
    public static class Game
    {
        // static members
        // fields
        static State state = State.Start;
        static int dealer = 0;
        static int smallBlind = 0;
        static int bigBlind = 0;

        // constants
        public const int BOARD_SIZE = 5;
        public const int HAND_SIZE = BOARD_SIZE + Hole.SIZE;
        public const int SMALL_BLIND = 10;
        public const int BIG_BLIND = 25;


        // properties
        public static State State
        {
            get { return state; }
            private set
            { 
                state = value >= State.Start && value < State.NextRound ?
                    value : state = State.Start;
            }
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
        /// increments state
        /// </summary>
        public static void NextState() { State++; }

        /// <summary>
        /// decrements state
        /// </summary>
        public static void PreviousState() { State--; }

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
