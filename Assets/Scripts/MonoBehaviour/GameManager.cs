using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace TexasHoldem.MonoScripts
{
    using static Game;
    using static Deck;

    /// <summary>
    /// MonoBehaviour in charge of moving the game
    /// forward and executing game play logic,
    /// this class also contains enumeration types
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // non-static members
        // fields
        [SerializeField] ObjectManager objectManager;
        [SerializeField] TMP_Text hand;
        [SerializeField] TMP_Text highCard;
        [SerializeField] TMP_Text kicker;
        InputAction nextState;
        readonly Card[] board = new Card[BOARD_SIZE];
        bool hasRoundStarted = false;

        // properties
        public static State State { get; private set; } = State.Start;
        public Player[] Players { get; private set; } = new Player[Player.MAX];

        // unity messages
        void Awake()
        {
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            nextState = InputSystem.actions.FindAction("Jump");

            Shuffle();
            DetermineOpeningDealer();
        }

        void Update()
        {
            if (State == State.NextRound)
                NextRound();

            if (nextState.WasPressedThisFrame())
                NextState();
        }

        // methods
        /// <summary>
        /// moves the game into the next state and calls functions dependaning on the state
        /// </summary>
        void NextState()
        {
            State++;


            switch (State)
            {
                case State.OpeningDeal:
                    if (!hasRoundStarted)
                    {
                        State--;
                        break;
                    }
                    DetermineOpeningDealer();
                    Player.SetBlinds();
                    SetBlindStates();
                    objectManager.InstantiateButtons();
                    State++;
                    break;
                case State.Deal:
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
        }

        /// <summary>
        /// sets blind states
        /// </summary>
        void SetBlindStates()
        {
            Players[Player.DealerIndex].Blind = Blind.Dealer;
            Players[Player.SmallBlindIndex].Blind = Blind.Small;
            Players[Player.BigBlindIndex].Blind = Blind.Big;
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
                    objectManager.InstantiatePlayerCard(playerIndex, Players[playerIndex].CardCount == 1);
                    playerIndex++;
                }
                else
                {
                    objectManager.InstantiateBoardCard(CardIndex - playerCardCount);
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
            //print($"{Players[0].Name}: {Players[0].Hand.GetName()} : {Players[0].HighCard} : {Players[0].Kicker}");

            bestHand = Players[0].Hand;
            highCard = Players[0].HighCard;
            kicker = Players[0].Kicker;

            for (int index = 1; index < Player.Count; index++)
            {
                Players[index].SetHand(board);
                //print($"{Players[index].Name}: {Players[index].Hand.GetName()}");

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

            hand.text = $"Winning Hand:\n{bestHand.GetName()}";
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
                    //print($"{Players[index].Name} has the best hand");
                }
            }

            //print("pot is divided " + potDivision + (potDivision == 1 ? " way" : " ways"));
        }

        /// <summary>
        /// resets gameState to RoundStart
        /// </summary>
        void NextRound()
        {
            State = State.RoundStart;
            hasRoundStarted = false;

            foreach (Player player in Players)
                player.Discard();

            objectManager.DestroyButtons();
            Player.NextDealer();
            objectManager.InstantiateButtons();

            hand.text = string.Empty;
            highCard.text = string.Empty;
            kicker.text = string.Empty;
        }

        // event methods
        public void SetPlayerReference(int index)
        {
            Players[index] = objectManager.Players[index].Player;
        }

        public void RoundStart()
        {
            hasRoundStarted = Player.Count >= Player.MIN;
            print($"{hasRoundStarted} : {Player.Count}");
            if (!hasRoundStarted)
                throw new System.Exception("Add at least two players");

            //NextState();
        }
    }
}
