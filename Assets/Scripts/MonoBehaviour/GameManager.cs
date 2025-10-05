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
        Clubs,
        Diamonds,
        Hearts,
        Spades
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
        Small,
        Big
    }
    // constants
    public const byte MAX_PLAYERS = 10;
    public const byte BOARD_SIZE = 5;
    public const byte HAND_SIZE = BOARD_SIZE + Hole.SIZE;
    // private members
    Deck deck = new Deck();
    Card[] board = new Card[BOARD_SIZE];
    Player[] players = new Player[MAX_PLAYERS];

    private void Awake()
    {
        Application.targetFrameRate = 60;

        for (int index = 0; index < MAX_PLAYERS; index++) // test loop adding players
            players[index] = new Player($"Player {index + 1}");
    }

    void DetermineOpeningDealer()
    {
        Card[] cards = new Card[Player.Count];

        for (int index = 0; index < Player.Count; index++)
            cards[index] = new Card(deck.CardIds[index]);

        print($"{players[0].Name}: {cards[0].Name}");
        for (int index = 1; index < Player.Count; index++)
        {
            print($"{players[index].Name}: {cards[index].Name}");
            if (cards[index].Rank > cards[Player.DealerIndex].Rank ||
                cards[index].Rank == cards[Player.DealerIndex].Rank &&
                cards[index].Suit > cards[Player.DealerIndex].Suit)
                Player.DealerIndex = index;
        }
        print($"{players[Player.DealerIndex].Name}: dealer");
    }

    void SetBlinds()
    {
        switch (Player.Count - Player.DealerIndex)
        {
            case 1:
                players[0].Blind = Blind.Small;
                players[1].Blind = Blind.Big;
                print($"{players[0].Name}: small blind");
                print($"{players[1].Name}: big blind");
                break;
            case 2:
                players[Player.Count - 1].Blind = Blind.Small;
                players[0].Blind = Blind.Big;
                print($"{players[Player.Count - 1].Name}: small blind");
                print($"{players[0].Name}: big blind");
                break;
            default:
                players[Player.DealerIndex + 1].Blind = Blind.Small;
                players[Player.DealerIndex + 2].Blind = Blind.Big;
                print($"{players[Player.DealerIndex + 1].Name}: small blind");
                print($"{players[Player.DealerIndex + 2].Name}: big blind");
                break;
        }
    }

    void Deal()
    {
        int playerCardCount = Hole.SIZE * Player.Count;
        for (int cardIndex = 0, playerIndex = Player.DealerIndex + 1; cardIndex < BOARD_SIZE + playerCardCount; cardIndex++)
        {
            if (cardIndex < playerCardCount)
            {
                if (playerIndex == Player.Count)
                    playerIndex = 0;

                players[playerIndex].AddCard(deck.CardIds[cardIndex]);
                playerIndex++;
            }
            else
                board[cardIndex - playerCardCount] = new Card(deck.CardIds[cardIndex]);
        }
    }
}
