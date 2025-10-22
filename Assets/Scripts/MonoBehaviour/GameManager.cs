using UnityEngine;
using UnityEngine.InputSystem;
using static Deck;

public class GameManager : MonoBehaviour
{
    // constants
    public const int MAX_PLAYERS = 10;
    public const int BOARD_SIZE = 5;
    public const int HAND_SIZE = BOARD_SIZE + Hole.SIZE;

    // static properties
    public static State GameState { get; private set; } = State.None;

    // non-static members
    [SerializeField] CardDealer cardDealer;
    InputAction nextState;
    Card[] board = new Card[BOARD_SIZE];

    // non-static properties
    public Player[] Players { get; private set; } = new Player[MAX_PLAYERS];

    // unity messages
    void Awake()
    {
        Application.targetFrameRate = 60;

        for (int index = 0; index < MAX_PLAYERS; index++) // test loop adding Players
            Players[index] = new Player($"Player {index + 1}");
    }

    void Start()
    {
        nextState = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        if (GameState == State.Reset)
            GameReset();

        if (nextState.WasPressedThisFrame())
            NextState();
    }

    // non-static methods
    void NextState()
    {
        GameState++;
        switch (GameState)
        {
            case State.Deal:
                Shuffle();
                DetermineOpeningDealer();
                SetBlinds();
                Shuffle();
                Deal();
                break;
            case State.Flip:
                DetermineWinner();
                break;
        }
    }

    void DetermineOpeningDealer()
    {
        Card[] cards = new Card[Player.Count];

        Shuffle();

        for (; CardIndex < Player.Count; NextCard())
            cards[CardIndex] = new Card(CardIds[CardIndex]);

        for (int index = 1; index < Player.Count; index++)
        {
            if (cards[index].Rank > cards[Player.DealerIndex].Rank ||
                cards[index].Rank == cards[Player.DealerIndex].Rank &&
                cards[index].Suit > cards[Player.DealerIndex].Suit)
                Player.DealerIndex = index;
        }
    }

    void SetBlinds()
    {
        switch (Player.Count - Player.DealerIndex)
        {
            case 1:
                Players[0].Blind = Blind.Small;
                Players[1].Blind = Blind.Big;
                break;
            case 2:
                Players[Player.Count - 1].Blind = Blind.Small;
                Players[0].Blind = Blind.Big;
                break;
            default:
                Players[Player.DealerIndex + 1].Blind = Blind.Small;
                Players[Player.DealerIndex + 2].Blind = Blind.Big;
                break;
        }
    }

    void Deal()
    {
        int playerCardCount = Hole.SIZE * Player.Count;

        Shuffle();

        for (int playerIndex = Player.DealerIndex + 1; CardIndex < BOARD_SIZE + playerCardCount; NextCard())
        {
            if (CardIndex < playerCardCount)
            {
                if (playerIndex == Player.Count)
                    playerIndex = 0;

                cardDealer.InstantiateCard(playerIndex, Players[playerIndex].CardCount);
                Players[playerIndex].AddCard(CardIds[CardIndex]);
                playerIndex++;
            }
            else
            {
                cardDealer.InstantiateCard(CardIndex - playerCardCount);
                board[CardIndex - playerCardCount] = new Card(CardIds[CardIndex]);
            }
        }
    }

    void DetermineWinner()
    {
        Hand bestHand;
        Rank highCard, kicker;
        int potDivision = 0;

        Players[0].SetHand(board);
        print($"{Players[0].Name}: {Players[0].FullHand} : {Players[0].HighCard} : {Players[0].Kicker}");

        bestHand = Players[0].Hand;
        highCard = Players[0].HighCard;
        kicker = Players[0].Kicker;

        for (int index = 1; index < Player.Count; index++)
        {
            Players[index].SetHand(board);
            print($"{Players[index].Name}: {Players[index].FullHand}");

            if (Players[index].Hand > bestHand)
            {
                bestHand = Players[index].Hand;
                highCard = Players[index].HighCard;
                kicker = Players[index].Kicker;
            }
            else if (Players[index].Hand == bestHand)
            {
                if (Players[index].HighCard > highCard)
                {
                    highCard = Players[index].HighCard;
                    kicker = Players[index].Kicker;
                }
                else if (Players[index].HighCard == highCard &&
                    Players[index].Kicker > kicker)
                    kicker = Players[index].Kicker;
            }
        }

        print($"best hand: {bestHand}, high card: {highCard}, kicker: {kicker}");

        for (int index = 0; index < Player.Count; index++)
        {
            if (Players[index].Hand == bestHand &&
                Players[index].HighCard == highCard &&
                Players[index].Kicker == kicker)
            {
                Players[index].HasBestHand = true;
                potDivision++;
                print($"{Players[index].Name} has the best hand");
            }
        }

        print("pot is divided " + potDivision + (potDivision == 1 ? " way" : " ways"));
    }

    void GameReset()
    {
        GameState = State.None;

        Player.ResetCount();

        for (int index = 0; index < MAX_PLAYERS; index++) // test loop replacing Players
            Players[index] = new Player($"Player {index + 1}");
    }

    // enums
    public enum State
    {
        None,
        Deal,
        Flip,
        Reset
    }

    // card ranks
    public enum Rank
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
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }

    // hand combinations
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

    public enum Blind
    {
        None,
        Small,
        Big
    }
}
