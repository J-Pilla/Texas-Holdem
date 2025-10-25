using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace TexasHoldem
{
    using static Deck;
    /// <summary>
    /// MonoBehaviour in charge of moving the game
    /// forward and executing game play logic,
    /// this class also contains enumeration types
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // static members
        // constants
        public const int BOARD_SIZE = 5;
        public const int HAND_SIZE = BOARD_SIZE + Hole.SIZE;

        // properties
        public static State GameState { get; private set; } = State.None;

        // non-static members
        // fields
        [SerializeField] CardDealer cardDealer;
        [SerializeField] TMP_Text hand;
        [SerializeField] TMP_Text highCard;
        [SerializeField] TMP_Text kicker;
        InputAction nextState;
        readonly Card[] board = new Card[BOARD_SIZE];

        // properties
        public Player[] Players { get; private set; } = new Player[Player.MAX];

        // unity messages
        void Awake()
        {
            Application.targetFrameRate = 60;
#pragma warning disable CS0612
            // test loop adding Players
            for (int index = 0; index < Player.MAX; index++)
                Players[index] = new Player();
#pragma warning restore CS0612
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

        // methods
        /// <summary>
        /// moves the game into the next state and calls functions dependaning on the state
        /// </summary>
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

        /// <summary>
        /// determine the opening dealer by dealing out a card
        /// to each player, the high card is the dealer
        /// </summary>
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
                    Player.SetInitialDealer(index);
            }

            cardDealer.InstantiateDealerButton(Player.DealerIndex);
            print($"Opening dealer is {Players[Player.DealerIndex].Name}");
        }

        /// <summary>
        /// sets big and small blinds following the dealer
        /// </summary>
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

        /// <summary>
        /// deals two cards to each player
        /// </summary>
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

                    Players[playerIndex].AddCard(CardIds[CardIndex]);
                    cardDealer.InstantiatePlayerCard(playerIndex, Players[playerIndex].CardCount == 1);
                    playerIndex++;
                }
                else
                {
                    cardDealer.InstantiateBoardCard(CardIndex - playerCardCount);
                    board[CardIndex - playerCardCount] = new Card(CardIds[CardIndex]);
                }
            }
        }

        /// <summary>
        /// finds the best hand among the players and settles the pot
        /// </summary>
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

            hand.text = $"Winning Hand:\n{bestHand}";
            this.highCard.text = $"High Card:\n{highCard}";
            this.kicker.text = $"Kicker:\n{kicker}";

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

        /// <summary>
        /// resets the game to the initial state
        /// </summary>
        void GameReset()
        {
            GameState = State.None;

            Player.ResetCount();
#pragma warning disable CS0612
            for (int index = 0; index < Player.MAX; index++) // test loop replacing Players
                Players[index] = new Player();
#pragma warning restore CS0612
            cardDealer.DestroyDealerButton();

            hand.text = string.Empty;
            highCard.text = string.Empty;
            kicker.text = string.Empty;
        }

        // enums
        /// <summary>
        /// game states
        /// </summary>
        public enum State
        {
            None,
            Deal,
            Flip,
            Reset
        }
    }
}
