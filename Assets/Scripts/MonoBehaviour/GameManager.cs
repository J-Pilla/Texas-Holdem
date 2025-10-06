using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Deck;

public class GameManager : MonoBehaviour
{
    // constants
    public const byte MAX_PLAYERS = 10;
    public const byte BOARD_SIZE = 5;
    public const byte HAND_SIZE = BOARD_SIZE + Hole.SIZE;
    // static properties
    public static State GameState { get; private set; } = State.None;
    // serialized members
    [SerializeField] CardDealer m_cardDealer;
    // private members
    InputAction nextState;

    Card[] board = new Card[BOARD_SIZE];
    Player[] players = new Player[MAX_PLAYERS];
    // properties
    public Player[] Players { get { return players; } }
    // unity messages
    private void Awake()
    {
        InitializeDeck();
        Application.targetFrameRate = 60;

        for (int index = 0; index < MAX_PLAYERS; index++) // test loop adding players
            players[index] = new Player($"Player {index + 1}");
    }

    private void Start()
    {
        nextState = InputSystem.actions.FindAction("Jump");
    }

    private void Update()
    {
        if (GameState == State.Reset)
            GameReset();

        if (nextState.WasPressedThisFrame())
            NextState();
    }
    // private methods
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

        for (; CardIndex < Player.Count; CardIndex++)
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
                players[0].Blind = Blind.Small;
                players[1].Blind = Blind.Big;
                break;
            case 2:
                players[Player.Count - 1].Blind = Blind.Small;
                players[0].Blind = Blind.Big;
                break;
            default:
                players[Player.DealerIndex + 1].Blind = Blind.Small;
                players[Player.DealerIndex + 2].Blind = Blind.Big;
                break;
        }
    }

    void Deal()
    {
        int playerCardCount = Hole.SIZE * Player.Count;

        Shuffle();

        for (int playerIndex = Player.DealerIndex + 1; CardIndex < BOARD_SIZE + playerCardCount; CardIndex++)
        {
            if (CardIndex < playerCardCount)
            {
                if (playerIndex == Player.Count)
                    playerIndex = 0;

                m_cardDealer.InstantiateCard(playerIndex, players[playerIndex].CardCount);
                players[playerIndex].AddCard(CardIds[CardIndex]);
                playerIndex++;
            }
            else
            {
                m_cardDealer.InstantiateCard(CardIndex - playerCardCount);
                board[CardIndex - playerCardCount] = new Card(CardIds[CardIndex]);
            }
        }
    }

    void DetermineWinner()
    {
        Hand bestHand;
        Rank highCard, kicker;
        int potDivision = 0;

        players[0].SetHand(board);
        print($"{players[0].Name}: {players[0].FullHand} : {players[0].HighCard} : {players[0].Kicker}");

        bestHand = players[0].Hand;
        highCard = players[0].HighCard;
        kicker = players[0].Kicker;

        for (int index = 1; index < Player.Count; index++)
        {
            players[index].SetHand(board);
            print($"{players[index].Name}: {players[index].FullHand}");

            if (players[index].Hand > bestHand)
            {
                bestHand = players[index].Hand;
                highCard = players[index].HighCard;
                kicker = players[index].Kicker;
            }
            else if (players[index].Hand == bestHand)
            {
                if (players[index].HighCard > highCard)
                {
                    highCard = players[index].HighCard;
                    kicker = players[index].Kicker;
                }
                else if (players[index].HighCard == highCard &&
                    players[index].Kicker > kicker)
                    kicker = players[index].Kicker;
            }
        }

        print($"best hand: {bestHand}, high card: {highCard}, kicker: {kicker}");

        for (int index = 0; index < Player.Count; index++)
        {
            if (players[index].Hand == bestHand &&
                players[index].HighCard == highCard &&
                players[index].Kicker == kicker)
            {
                players[index].HasBestHand = true;
                potDivision++;
                print($"{players[index].Name} has the best hand");
            }
        }

        print("pot is divided " + potDivision + (potDivision == 1 ? " way" : " ways"));
    }

    void GameReset()
    {
        GameState = State.None;

        Player.ResetCount();

        for (int index = 0; index < MAX_PLAYERS; index++) // test loop replacing players
            players[index] = new Player($"Player {index + 1}");
    }
    // enums
    public enum State : byte
    {
        None,
        Deal,
        Flip,
        Reset
    }
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
}
