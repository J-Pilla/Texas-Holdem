using TMPro;
using UnityEngine;

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
        [SerializeField] GameObject dealerButtonPrefab;
        [SerializeField] GameObject smallBlindButtonPrefab;
        [SerializeField] GameObject bigBlindButtonPrefab;
        // game objects instantiated from prefabs
        GameObject dealerButton;
        GameObject smallBlindButton;
        GameObject bigBlindButton;
        // prefab component storage
        readonly PlayerScript[] players = new PlayerScript[Player.MAX];
        readonly Card[] board = new Card[BOARD_SIZE];
        [Header("Game Object Containers")]
        [SerializeField] GameObject seatTargets;
        [SerializeField] GameObject boardTargets;
        // game object component storage
        readonly Transform[] seatTransforms = new Transform[Player.MAX];
        readonly Transform[] boardTransforms = new Transform[Player.MAX];
        [Header("UI Containers")]
        [SerializeField] GameObject seats;
        [SerializeField] GameObject roundStart;
        [SerializeField] GameObject winDisplay;
        [Header("UI Objects")]
        [Header("Round Start")]
        [SerializeField] GameObject roundStartError;
        [Header("UI Components")]
        [Header("Player Display")]
        [SerializeField] TMP_Text[] nameDisplays = new TMP_Text[Player.MAX];
        [SerializeField] TMP_Text[] chipDisplays = new TMP_Text[Player.MAX];
        [Header("Round Start")]
        [SerializeField] TMP_Text roundStartText;
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
            SetTransforms(seatTransforms);
            SetTransforms(boardTransforms);
        }

        void Update()
        {
            if (State == State.NextRound)
                NextRound();
        }

        // methods
        /// <summary>
        /// initializes all the elements of the transform arrays
        /// </summary>
        void SetTransforms(Transform[] targetTransforms)
        {
            GameObject targets = targetTransforms == seatTransforms ?
                seatTargets : boardTargets;

            Transform[] childTransforms = targets.GetComponentsInChildren<Transform>();
            
            int index = 0;
            foreach (Transform transform in childTransforms)
            {
                if (transform.gameObject != targets)
                {
                    targetTransforms[index] = transform;
                    index++;
                }
            }
        }

        /// <summary>
        /// moves the game into the next state and calls functions dependaning on the state
        /// </summary>
        public void NextState()
        {
            Game.NextState();


            switch (State)
            {
                case State.RoundStart:
                    RoundStart();

                    if (!isDealerDetermined)
                        DetermineOpeningDealer();
                    else
                    {
                        DestroyButtons();
                        Player.NextDealer();
                    }
                    SetBlinds();
                    SetRoundStartToDeal();
                    break;
                case State.Deal:
                    HideRoundStart();
                    Discard();
                    Deal();
                    NextState();
                    break;
                case State.Flip:
                    FlipCards();
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
                PreviousState();
                throw new System.Exception("Add at least two players");
            }

            seats.SetActive(false);
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
                players[CardIndex].AddCard(
                    CardIds[CardIndex],
                    cardPrefab,
                    players[CardIndex].gameObject.transform,
                    true);

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
        }

        /// <summary>
        /// sets blinds
        /// </summary>
        void SetBlinds()
        {
            Player.SetBlinds();

            players[Player.DealerIndex].Blind = Blind.Dealer;
            InstantiateButton(Blind.Dealer);

            players[Player.SmallBlindIndex].Blind = Blind.Small;
            InstantiateButton(Blind.Small);

            if (Player.Count > Player.MIN)
            {
                players[Player.BigBlindIndex].Blind = Blind.Big;
                InstantiateButton(Blind.Big);
            }
        }

        /// <summary>
        /// instantiates the daeler and blind buttons
        /// </summary>
        /// <param name="blind"></param>
        void InstantiateButton(Blind blind)
        {
            (int index, GameObject buttonPrefab) = blind switch
            {
                Blind.Small => (Player.SmallBlindIndex, smallBlindButtonPrefab),
                Blind.Big => (Player.BigBlindIndex, bigBlindButtonPrefab),
                _ => (Player.DealerIndex, dealerButtonPrefab)
            };
            GameObject button = Instantiate(
                buttonPrefab,
                seatTransforms[players[index].Seat]);

            button.transform.localPosition += new Vector3(-.9f, .3f);

            switch (blind)
            {
                case Blind.Small:
                    smallBlindButton = button;
                    break;
                case Blind.Big:
                    bigBlindButton = button;
                    break;
                default:
                    dealerButton = button;
                    break;
            }
        }

        /// <summary>
        /// destroys the dealer and blind buttons
        /// </summary>
        public void DestroyButtons()
        {
            Destroy(dealerButton);
            Destroy(smallBlindButton);
            Destroy(bigBlindButton);
        }

        /// <summary>
        /// changes the text of the Round Start UI elements to show it's time to deal
        /// </summary>
        void SetRoundStartToDeal()
        {
            roundStartText.text = "Deal";
            dealer.text = $"{players[Player.DealerIndex].Name}'s turn to deal!";
        }

        /// <summary>
        /// reverts and hides the Round Start UI elements
        /// </summary>
        void HideRoundStart()
        {
            roundStart.SetActive(false);
            roundStartText.text = "Round\nStart";
            dealer.text = string.Empty;
        }

        /// <summary>
        /// discards each players cards
        /// </summary>
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
                    playerIndex++;
                }
                else
                {
                    board[CardIndex - playerCardCount] =
                        new Card(CardIds[CardIndex],
                            false,
                            cardPrefab,
                            boardTransforms[CardIndex - playerCardCount]);
                }
            }
        }

        /// <summary>
        /// flips all the cards on the table
        /// </summary>
        void FlipCards()
        {
            for (int index = 0; index < Player.Count; index++)
                players[index].FlipCards();

            foreach (Card card in board)
                card.FlipCard();
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
            NextState();
            hasRoundStarted = false;

            Discard();

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
            players[Player.Count] = Instantiate(playerPrefab, seatTransforms[seat]).GetComponent<PlayerScript>();
            players[Player.Count - 1].Seat = seat;
        }

        /// <summary>
        /// sets the player's name, call AFTER AddPlayer
        /// </summary>
        /// <param name="inputField"></param>
        public void SetPlayerName(TMP_InputField inputField)
        {
            if (inputField.text != string.Empty)
            {
                players[Player.Count - 1].Name = inputField.text;
                nameDisplays[players[Player.Count - 1].Seat].text = inputField.text;
                chipDisplays[players[Player.Count - 1].Seat].text = players[Player.Count - 1].Chips.ToString();
            }
            else
            {
                Destroy(players[Player.Count - 1].gameObject);
                throw new System.Exception("Add text to input field");
            }
        }

        // Leave Table event
        /// <summary>
        /// destroys a player game object
        /// </summary>
        /// <param name="seat"></param>
        public void LeaveTable(int seat)
        {
            GameObject player = seatTransforms[seat].gameObject.GetComponentInChildren<PlayerScript>().gameObject;
            Player.DecrementCount();
            Destroy(player);
        }
    }
}
