using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem.MonoScripts
{
    using static Deck;
    using static Game;

    /// <summary>
    /// MonoBehaviour in charge of moving the game forward and executing game play logic
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // fields
        bool hasRoundStarted = false;
        bool isDealerDetermined = false;
        [Header("Prefabs")]
        [SerializeField] GameObject playerPrefab;
        [SerializeField] GameObject cardPrefab;
        // prefab component storage
        readonly PlayerScript[] players = new PlayerScript[Player.MAX];
        readonly Card[] board = new Card[BOARD_SIZE];
        [Header("Game Objects")]
        [SerializeField] GameObject seatTargets;
        [SerializeField] ObjectManager objectManager;
        // game object component storage
        readonly Transform[] seats = new Transform[Player.MAX];
        [Header("UI Containers")]
        [SerializeField] GameObject uISeats;
        [SerializeField] GameObject roundStart;
        [SerializeField] GameObject deal;
        [SerializeField] GameObject winDisplay;
        [Header("UI Objects")]
        [Header("Round Start")]
        [SerializeField] GameObject roundStartError;
        [Header("UI Components")]
        [Header("Player Display")]
        [SerializeField] TMP_Text[] nameDisplays = new TMP_Text[Player.MAX];
        [SerializeField] TMP_Text[] chipDisplays = new TMP_Text[Player.MAX];
        [Header("Round Start")]
        [SerializeField] TMP_Text dealer;
        [Header("Win Display")]
        [SerializeField] TMP_Text hand;
        [SerializeField] TMP_Text highCard;
        [SerializeField] TMP_Text kicker;

        // unity messages
        void Awake()
        {
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            SetSeats();
        }

        void Update()
        {
            if (State == State.NextRound)
                NextRound();
        }

        // methods
        /// <summary>
        /// initializes all the elements of seats array
        /// </summary>
        void SetSeats()
        {
            Transform[] transforms = seatTargets.GetComponentsInChildren<Transform>();
            int index = 0;
            foreach (Transform transform in transforms)
            {
                if (transform.gameObject != seatTargets)
                {
                    seats[index] = transform;
                    index++;
                }
            }
        }

        /// <summary>
        /// moves the game into the next state and calls functions dependaning on the state
        /// </summary>
        public void NextState()
        {
            State++;


            switch (State)
            {
                case State.RoundStart:
                    RoundStart();

                    if (!isDealerDetermined)
                        DetermineOpeningDealer();
                    else
                        Player.NextDealer();
                    
                    SetBlinds();
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
        /// attempts to start the round, if their is enough players,
        /// deactivate the UI Seats container and the Round Start UI Object
        /// </summary>
        /// <exception cref="System.Exception"></exception>
        public void RoundStart()
        {
            hasRoundStarted = Player.Count >= Player.MIN;

            roundStartError.SetActive(!hasRoundStarted);

            if (!hasRoundStarted)
            {
                State--;
                throw new System.Exception("Add at least two players");
            }

            uISeats.SetActive(false);
            roundStart.GetComponentInChildren<Button>().gameObject.SetActive(false);
        }

        /// <summary>
        /// determine the opening dealer by dealing out a card
        /// to each player, the high card is the dealer
        /// </summary>
        void DetermineOpeningDealer()
        {
            Shuffle();

            for (; CardIndex < Player.Count; NextCard())
            {
                players[CardIndex].AddCard(CardIds[CardIndex], cardPrefab, players[CardIndex].gameObject.transform);
                players[CardIndex].Cards[0].FlipCard();
            }

            for (int index = 1; index < Player.Count; index++)
            {
                if (players[index].Cards[0].Rank > players[Player.DealerIndex].Cards[0].Rank ||
                    players[index].Cards[0].Rank == players[Player.DealerIndex].Cards[0].Rank &&
                    players[index].Cards[0].Suit > players[Player.DealerIndex].Cards[0].Suit)
                    Player.SetInitialDealer(index);
            }

            isDealerDetermined = true;
            dealer.text = $"{players[Player.DealerIndex].Name}'s turn to deal!";
        }

        /// <summary>
        /// sets blinds
        /// </summary>
        void SetBlinds()
        {
            Player.SetBlinds();

            players[Player.DealerIndex].Blind = Blind.Dealer;
            players[Player.SmallBlindIndex].Blind = Blind.Small;

            if (Player.Count > Player.MIN)
                players[Player.BigBlindIndex].Blind = Blind.Big;
        }

        void Discard()
        {
            for (int index = 0; index < Player.Count; index++)
                players[index].Discard();
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

                    players[playerIndex].AddCard(CardIds[CardIndex], cardPrefab, players[playerIndex].gameObject.transform);
                    objectManager.InstantiatePlayerCard(playerIndex, players[playerIndex].CardCount == 1);
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

            players[0].SetHand(board);
            //print($"{players[0].Name}: {players[0].Hand.GetName()} : {players[0].HighCard} : {players[0].Kicker}");

            bestHand = players[0].Hand;
            highCard = players[0].HighCard;
            kicker = players[0].Kicker;

            for (int index = 1; index < Player.Count; index++)
            {
                players[index].SetHand(board);
                //print($"{players[index].Name}: {players[index].Hand.GetName()}");

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

            hand.text = $"Winning Hand:\n{bestHand.GetName()}";
            this.highCard.text = $"High Card:\n{highCard}";
            this.kicker.text = $"Kicker:\n{kicker}";

            for (int index = 0; index < Player.Count; index++)
            {
                if (players[index].Hand == bestHand &&
                    players[index].HighCard == highCard &&
                    players[index].Kicker == kicker)
                {
                    players[index].HasBestHand = true;
                    potDivision++;
                    //print($"{players[index].Name} has the best hand");
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

            foreach (PlayerScript player in players)
                player.Discard();

            objectManager.DestroyButtons();
            Player.NextDealer();
            objectManager.InstantiateButtons();

            hand.text = string.Empty;
            highCard.text = string.Empty;
            kicker.text = string.Empty;
        }

        // event methods
        // "Sit" button events
        /// <summary>
        /// instantiates a player game object, call BEFORE SetPlayerName
        /// </summary>
        /// <param name="seat"></param>
        public void AddPlayer(int seat)
        {
            players[Player.Count] = Instantiate(playerPrefab, seats[seat]).GetComponent<PlayerScript>();
            players[Player.Count - 1].Seat = seat;
        }

        /// <summary>
        /// sets the player's name, call AFTER AddPlayer
        /// </summary>
        /// <param name="inputField"></param>
        public void SetPlayerName(TMP_InputField inputField)
        {
            if (inputField.text != string.Empty)
                players[Player.Count - 1].Name = inputField.text;
            else
            {
                Destroy(players[Player.Count - 1].gameObject);
                throw new System.Exception("Add text to input field");
            }
        }

        // Leave Table and Cancel event
        /// <summary>
        /// destroys a player game object
        /// </summary>
        /// <param name="seat"></param>
        public void LeaveTable(int seat)
        {
            GameObject player = seats[seat].gameObject.GetComponentInChildren<PlayerScript>().gameObject;
            Player.DecrementCount();
            Destroy(player);
        }
    }
}
