using UnityEngine;

public class GameManager : MonoBehaviour
{
    // card ranks
    public enum Rank : byte
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
    // card suits
    public enum Suit : byte
    {
        Spades,
        Hearts,
        Clubs,
        Diamonds
    }
    // hand combinations
    public enum Hand : byte
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

    public enum Blind : byte
    {
        None,
        Dealer,
        Small,
        Big
    }

    public const byte BOARD_SIZE = 5;
    public const byte HAND_SIZE = BOARD_SIZE + Hole.SIZE;
}
