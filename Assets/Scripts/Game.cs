namespace TexasHoldem
{
    public static class Game
    {
        // static members
        // constants
        public const int BOARD_SIZE = 5;
        public const int HAND_SIZE = BOARD_SIZE + Hole.SIZE;

        // enums
        /// <summary>
        /// game states
        /// </summary>
        public enum State
        {
            Start,
            OpeningDeal,
            RoundStart,
            Deal,
            Flip,
            NextRound
        }
    }
}
