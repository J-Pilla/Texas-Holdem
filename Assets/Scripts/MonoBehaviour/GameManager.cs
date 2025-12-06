using System.Collections;
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

        [Header("Game Object Components")]
        [SerializeField] Camera boardCamera;

        // game object component storage
        readonly Transform[] seatTransforms = new Transform[Player.MAX];
        readonly Transform[] boardTransforms = new Transform[Player.MAX];

        [Header("UI Containers")]
        [Header("Main UI")]
        [SerializeField] GameObject[] addLeaveControls = new GameObject[Player.MAX];
        [SerializeField] GameObject roundStart;
        [SerializeField] GameObject winDisplay;
        [Header("Focus UI")]
        [SerializeField] GameObject pTLabels;
        [SerializeField] GameObject betLabel;
        [SerializeField] GameObject focusControls;
        [SerializeField] GameObject fCRControls;
        [SerializeField] GameObject cBControls;

        // ui container coordinates
        float betLabelYCoordinate;
        float focusContolsXCoordinate;

        [Header("UI Objects")]
        [Header("Round Start")]
        [SerializeField] GameObject roundStartError;
        [Header("FCR Controls")]
        [SerializeField] GameObject raiseButton;
        [Header("CB Controls")]
        [SerializeField] GameObject betButton;

        [Header("UI Components")]
        [Header("Main UI")]
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
        [Header("Focus UI")]
        [Header("Focus Display")]
        [SerializeField] TMP_Text potDisplay;
        [SerializeField] TMP_Text topBetDisplay;
        [SerializeField] TMP_Text betDisplay;
        [Header("Focus Controls")]
        [SerializeField] TMP_InputField betInput;
        [SerializeField] TMP_InputField passwordInput;

        // unity messages
        void Awake()
        {
            Application.targetFrameRate = 60;
        }

        void Start()
        {
            SetInitialProjectionSize(Camera.main.orthographicSize);
            SetTransforms(seatTransforms);
            SetTransforms(boardTransforms);
            SetUICoordinates();
        }

        // start methods
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

        void SetUICoordinates()
        {
            betLabelYCoordinate = betLabel.transform.localPosition.y;
            focusContolsXCoordinate = focusControls.transform.localPosition.x;
        }

        void Update()
        {
            if (State == State.NextRound)
                NextRound();
        }

        // update methods
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
                    SortPlayersBySeat();
                    if (!IsDealerDetermined)
                        DetermineOpeningDealer();
                    else
                    {
                        DestroyButtons();
                        NextDealer();
                    }
                    InstantiateButton();
                    SetBlinds();
                    SetRoundStartToDeal();
                    break;
                case State.Deal:
                    HideRoundStart();
                    if (Round < 2)
                        Discard();
                    Deal();
                    NextState();
                    break;
                case State.PreFlop:
                    StartCameraTransform();
                    break;
                case State.Showdown:
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
            HasRoundStarted = Player.Count >= Player.MIN;

            roundStartError.SetActive(!HasRoundStarted);

            if (!HasRoundStarted)
            {
                PreviousState();
                throw new System.Exception("Add at least two players");
            }

            foreach (GameObject control in addLeaveControls)
                control.SetActive(false);
        }

        /// <summary>
        /// sorts players array by the seat the players are at
        /// so blinds are posted correctly and cards are dealt in order
        /// </summary>
        void SortPlayersBySeat()
        {
            for (int index1 = 0; index1 < Player.Count - 1; index1++)
                for (int index2 = index1 + 1; index2 < Player.Count; index2++)
                    if (players[index1].Seat > players[index2].Seat)
                        (players[index1], players[index2]) = (players[index2], players[index1]);
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
                if (players[index].Cards[0].Rank > players[Dealer].Cards[0].Rank ||
                    players[index].Cards[0].Rank == players[Dealer].Cards[0].Rank &&
                    players[index].Cards[0].Suit > players[Dealer].Cards[0].Suit)
                    SetInitialDealer(index);
            }

            IsDealerDetermined = true;
        }

        /// <summary>
        /// sets blinds
        /// </summary>
        void SetBlinds()
        {
            Game.SetBlinds();

            players[SmallBlind].Blind = Blind.Small;
            InstantiateButton(Blind.Small);
            PlaceBet(players[SmallBlind], SMALL_BLIND);

            players[BigBlind].Blind = Blind.Big;
            InstantiateButton(Blind.Big);
            PlaceBet(players[BigBlind], BIG_BLIND);

        }

        /// <summary>
        /// instantiates the daeler and blind buttons
        /// </summary>
        /// <param name="blind"></param>
        void InstantiateButton(Blind blind = Blind.None)
        {
            (int index, GameObject buttonPrefab) = blind switch
            {
                Blind.Small => (SmallBlind, smallBlindButtonPrefab),
                Blind.Big => (BigBlind, bigBlindButtonPrefab),
                _ => (Dealer, dealerButtonPrefab)
            };
            GameObject button = Instantiate(
                buttonPrefab,
                seatTransforms[players[index].Seat]);

            button.transform.localPosition += new Vector3(-.9f, .3f);
            switch (blind)
            {
                case Blind.Small:
                    if (SmallBlind == Dealer)
                        button.transform.localPosition += new Vector3(0f, -.6f);
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
            dealer.text = $"{players[Dealer].Name}'s turn to deal!";
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

            for (int playerIndex = Dealer + 1; CardIndex < BOARD_SIZE + playerCardCount; NextCard())
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

        void StartCameraTransform(bool isZooming = true)
        {
            StartCoroutine(MoveCamera());
            
            if (isZooming)
                StartCoroutine(ZoomCamera());

            StartCoroutine(RotateCamera());
            StartCoroutine(MoveBoardCamera());
        }

        /// <summary>
        /// move the camera using lerp
        /// </summary>
        IEnumerator MoveCamera()
        {
            Vector3 origin = Camera.main.transform.position;
            Vector3 destination = seatTransforms[players[Turn].Seat].position;
            destination.z = origin.z;

            if (destination != origin)
            {
                float startTime = Time.time;

                if (origin != new Vector3(0f, 0f, origin.z))
                {
                    betLabel.SetActive(false);
                    DeactivateFocusControls();
                }

                while (Time.time - startTime < CameraTransformDuration)
                {
                    Camera.main.transform.position =
                    Vector3.Lerp(
                        origin,
                        destination,
                        (Time.time - startTime) / CameraTransformDuration);

                    yield return null;
                }

                Camera.main.transform.position = destination;

                if (origin == new Vector3(0f, 0f, origin.z))
                    pTLabels.SetActive(true);

                if (destination != new Vector3(0f, 0f, destination.z))
                    SetFocusUI();
                else
                    pTLabels.SetActive(false);
            }
        }

        void DeactivateFocusControls()
        {
            focusControls.SetActive(false);
            fCRControls.SetActive(false);
            cBControls.SetActive(false);
        }

        /// <summary>
        /// sets up focus controls for a players turn
        /// </summary>
        void SetFocusUI()
        {
            int multiplier =
                players[Turn].Seat < 4 || players[Turn].Seat > 7 ? 1 : -1;

            betLabel.transform.localPosition = new(
                betLabel.transform.localPosition.x,
                betLabelYCoordinate * multiplier,
                betLabel.transform.localPosition.z);

            focusControls.transform.localPosition = new(
                focusContolsXCoordinate * multiplier,
                focusControls.transform.localPosition.y,
                focusControls.transform.localPosition.z);

            betDisplay.text = players[Turn].Bet.ToString();

            if (TopBet - players[Turn].Bet < players[Turn].Chips)
            {
                betInput.gameObject.SetActive(true);
                betInput.text = (TopBet + SMALL_BLIND).ToString();
                betInput.ActivateInputField();

                raiseButton.SetActive(true);
                betButton.SetActive(true);
            }
            else
            {
                betInput.gameObject.SetActive(false);
                raiseButton.SetActive(false);
                betButton.SetActive(false);
            }

            if (players[Turn].Bet == TopBet)
                cBControls.SetActive(true);
            else
                fCRControls.SetActive(true);

            passwordInput.gameObject.SetActive(players[Turn].Password != string.Empty);
            passwordInput.text = string.Empty;

            betLabel.SetActive(true);
            focusControls.SetActive(true);
        }

        /// <summary>
        /// zoom the camera using lerp 
        /// </summary>
        IEnumerator ZoomCamera()
        {
            float origin = Camera.main.orthographicSize;
            float destination = origin == InitialProjectionSize ?
                FocusProjectionSize : InitialProjectionSize;
            float startTime = Time.time;

            boardCamera.gameObject.SetActive(destination == FocusProjectionSize);

            while (Time.time - startTime < CameraTransformDuration)
            {
                Camera.main.orthographicSize = Mathf.Lerp(
                    origin,
                    destination,
                    (Time.time - startTime) / CameraTransformDuration);

                yield return null;
            }

            Camera.main.orthographicSize = destination;
        }

        /// <summary>
        /// rotate the camera using lerp 
        /// </summary>
        IEnumerator RotateCamera()
        {
            float origin = Camera.main.transform.eulerAngles.z switch
            {
                270f => -CameraAngles[1],
                330f => -CameraAngles[0],
                _ => Camera.main.transform.eulerAngles.z
            };
            float destination = players[Turn].Seat switch
            {
                2 or 7 => -CameraAngles[0],
                3 => -CameraAngles[1],
                4 or 9 => CameraAngles[0],
                8 => CameraAngles[1],
                _ => 0f
            };

            if (destination != origin)
            {
                float startTime = Time.time;

                while (Time.time - startTime < CameraTransformDuration)
                {
                    Camera.main.transform.eulerAngles = Vector3.Lerp(
                        Vector3.forward * origin,
                        Vector3.forward * destination,
                        (Time.time - startTime) / CameraTransformDuration);

                    yield return null;
                }

                Camera.main.transform.eulerAngles = Vector3.forward * destination;
            }
        }


        /// <summary>
        /// move the board camera using lerp
        /// </summary>
        IEnumerator MoveBoardCamera()
        {
            Vector2 origin = boardCamera.rect.position;
            Vector2 destination = new(boardCamera.rect.x,
                players[Turn].Seat < 4 || players[Turn].Seat > 7 ?
                BoardCamaraTop : BoardCamaraBottom);

            if (destination != origin)
            {
                float startTime = Time.time;

                while (Time.time - startTime < CameraTransformDuration)
                {
                    boardCamera.rect = new(Vector2.Lerp(
                        origin,
                        destination,
                        (Time.time - startTime) / CameraTransformDuration),
                        boardCamera.rect.size);

                    yield return null;
                }

                boardCamera.rect = new(destination, boardCamera.rect.size);
            }
        }

        void PlaceBet(PlayerScript player, int bet)
        {
            player.PlaceBet(bet);
            chipDisplays[player.Seat].text = player.Chips.ToString();

            UpdatePot(bet);
            if (player.Bet > TopBet)
                UpdateTopBet(player.Bet);
        }

        void UpdatePot(int bet)
        {
            AddToPot(bet);
            potDisplay.text = Pot.ToString();
        }

        void UpdateTopBet(int bet)
        {
            TopBet = bet;
            topBetDisplay.text = TopBet.ToString();
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
            winDisplay.SetActive(true);

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

        void NextTurn()
        {
            Game.NextTurn();

            if (players[Turn].Chips == 0 || players[Turn].Hand == Hand.Fold)
                NextTurn();

            StartCameraTransform(false);
        }

        /// <summary>
        /// resets gameState to RoundStart
        /// </summary>
        void NextRound()
        {
            Game.NextState();
            Game.NextRound();

            HasRoundStarted = false;

            Discard();

            winDisplay.SetActive(true);
            hand.text = string.Empty;
            highCard.text = string.Empty;
            kicker.text = string.Empty;
        }

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
            if (inputField.text == string.Empty)
            {
                Destroy(players[Player.Count - 1].gameObject);
                throw new System.Exception("Add text to input field");
            }

            players[Player.Count - 1].Name = inputField.text;
            nameDisplays[players[Player.Count - 1].Seat].text = inputField.text;
            chipDisplays[players[Player.Count - 1].Seat].text = players[Player.Count - 1].Chips.ToString();
        }

        /// <summary>
        /// sets the player's password, call AFTER AddPlayer
        /// </summary>
        /// <param name="inputField"></param>
        public void SetPlayerPassword(TMP_InputField inputField)
        {
            players[Player.Count - 1].Password = inputField.text;
        }

        // Leave Table event
        /// <summary>
        /// destroys a player game object
        /// </summary>
        /// <param name="seat"></param>
        public void LeaveTable(int seat)
        {
            int index = 0;

            for (; index < Player.Count; index++)
                if (players[index].Seat == seat)
                    break;

            Destroy(players[index].gameObject);

            for (; index < Player.Count; index++)
                players[index] = players[index + 1];

            players[index] = null;
        }

        // All In event
        /// <summary>
        /// sets a players bet to the maximum
        /// </summary>
        public void AllIn()
        {
            PlaceBet(players[Turn], players[Turn].Chips);
            NextTurn();
        }

        // Bet and Raise event
        /// <summary>
        /// increases the players bet beyond the current highest bet
        /// </summary>
        public void Bet()
        {
            int minimumBet = TopBet + SMALL_BLIND;
            int bet = betInput.text != string.Empty ?
                int.Parse(betInput.text) : minimumBet;

            bet -= players[Turn].Bet;

            if (bet >= players[Turn].Chips)
                PlaceBet(players[Turn], players[Turn].Chips);
            else
                PlaceBet(players[Turn],
                    bet >= minimumBet ? bet : minimumBet);

            NextTurn();
        }

        // Call event
        /// <summary>
        /// increases the players bet to match the current highest bet
        /// </summary>
        public void Call()
        {
            PlaceBet(players[Turn], TopBet - players[Turn].Bet);
            NextTurn();
        }

        // Fold event
        /// <summary>
        /// sets the players hand to fold
        /// </summary>
        public void Fold()
        {
            players[Turn].Fold();
            NextTurn();
        }

        // Check event
        public void Check() { NextTurn(); }

        // Toggle Cards event
        /// <summary>
        /// flips player cards until button is released
        /// </summary>
        /// <param name="inputField"></param>
        public void ToggleCards()
        {
            if (players[Turn].Password == string.Empty ||
                passwordInput.text == players[Turn].Password)
                players[Turn].FlipCards();
        }
    }
}
