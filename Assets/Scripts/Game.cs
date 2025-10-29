namespace TexasHoldem
{
    public static class Game
    {
        // static members
        // fields
        static State state = State.Start;

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

        // methods
        /// <summary>
        /// increments state
        /// </summary>
        public static void NextState() { State++; }

        /// <summary>
        /// decrements state
        /// </summary>
        public static void PreviousState() { State--; }
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
