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

        // properties
        public static State State
        {
            get { return state; }
            set
            {
                state = value < State.NextRound ? value : state = State.Start;
            }
        }

        // methods
        public static void NextState()
        {
            State++;
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
        Flip,
        NextRound
    }
}
