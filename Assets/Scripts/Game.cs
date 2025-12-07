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
        static int turn = 0;
        static int pot = 0;
        static int topBet = 0;
        static int activePlayers = 0;
        static int actionsTaken = 0;

        // constants
        public const int BOARD_SIZE = 5;
        public const int FLOP = 2;
        public const int TURN = 3;
        public const int RIVER = 4;
        public const int HAND_SIZE = BOARD_SIZE + Hole.SIZE;
        public const int SMALL_BLIND = 10;
        public const int BIG_BLIND = 25;

        // properties
        public static float CameraTransformDuration { get; } = 1f;
        public static float InitialProjectionSize { get; private set; }
        public static float FocusProjectionSize { get; } = 2.5f;
        public static float[] CameraAngles { get; } = { 30f, 90f };
        public static float BoardCamaraTop { get; } = .75f;
        public static float BoardCamaraBottom { get; } = 0f;
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
        public static int Turn
        {
            get { return turn; }
            private set { turn = value < Player.Count ? value : 0; }
        }
        public static int Pot
        {
            get { return pot; }
            private set { pot = value > 0 ? value : 0; }
        }
        public static int TopBet
        {
            get { return topBet; }
            set
            {
                if (value > topBet || value == 0)
                    topBet = value;
            }
        }
        public static int ActivePlayers
        {
            get { return activePlayers; }
            private set { activePlayers = value >= 0 ? value : 0; }
        }
        public static int ActionsTaken
        {
            get { return actionsTaken; }
            private set { actionsTaken = value; }
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
        /// sets the games state to the state before showdown
        /// </summary>
        public static void SkipToShowdown() { State = State.Showdown - 1; }

        /// <summary>
        /// increments round
        /// </summary>
        public static void NextRound() { Round++; }

        /// <summary>
        /// sets the initial dealer, should only be called on the first round
        /// </summary>
        /// <param name="dealerIndex"></param>
        public static void SetInitialDealer(int dealerIndex) { Dealer = dealerIndex; }

        /// <summary>
        /// increments dealer
        /// </summary>
        public static void NextDealer() { Dealer++; }

        /// <summary>
        /// set blinds
        /// </summary>
        public static void SetBlinds()
        {
            SmallBlind = Player.Count > 2 ? Dealer + 1 : Dealer;
            BigBlind = SmallBlind + 1;
            Turn = BigBlind + 1;
        }

        /// <summary>
        /// increments turn
        /// </summary>
        public static void NextTurn() { Turn++; }

        /// <summary>
        /// sets turn after the pre flop
        /// </summary>
        public static void SetTurn() { Turn = Dealer; }

        /// <summary>
        /// adds the sum of bets to the pot
        /// </summary>
        /// <param name="betSum"></param>
        public static void AddToPot(int betSum) { Pot += betSum; }

        /// <summary>
        /// reduces the pot during payout
        /// </summary>
        public static void ReducePot(int payout) { Pot -= payout; }

        /// <summary>
        /// sets active players to the amount of players
        /// </summary>
        public static void SetActivePlayers() { ActivePlayers = Player.Count; }

        /// <summary>
        /// decrements active players
        /// </summary>
        public static void DecrementActivePlayers() { ActivePlayers--; }

        /// <summary>
        /// sets active players to 0
        /// </summary>
        public static void ResetActivePlayers() { ActivePlayers = 0; }

        /// <summary>
        /// increments bets placed
        /// </summary>
        public static void IncrementActionsTaken() { ActionsTaken++; }

        /// <summary>
        /// clears the pot during payout
        /// </summary>
        public static void ResetActionsTaken() { ActionsTaken = 0; }
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
        PreFlop,
        Flop,
        Turn,
        River,
        Showdown,
        NextRound
    }
}
