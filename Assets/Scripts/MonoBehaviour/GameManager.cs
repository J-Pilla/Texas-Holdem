using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
        Card[] board = new Card[BOARD_SIZE];

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
        [Header("Main UI")]
        [SerializeField] GameObject roundStartError;
        [Header("Focus UI")]
        [SerializeField] GameObject raiseButton;
        [SerializeField] GameObject callButton;

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
        [SerializeField] TMP_Text winners;
        [Header("Focus UI")]
        [Header("Focus Display")]
        [SerializeField] TMP_Text potDisplay;
        [SerializeField] TMP_Text topBetDisplay;
        [SerializeField] TMP_Text betDisplay;
        [Header("Focus Controls")]
        [SerializeField] TMP_InputField betInput;
        [SerializeField] TMP_InputField passwordInput;

        // controls
        InputAction escape;
        InputAction quit;

        // unity messages
        void Awake()
        {
            Application.targetFrameRate = 60;
            escape = InputSystem.actions.FindAction("Escape");
            quit = InputSystem.actions.FindAction("Quit");
        }

        private void Update()
        {
            if (escape.IsPressed())
                if (quit.WasPressedThisFrame())
                    Application.Quit();

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

        // camera methods
        /// <summary>
        /// focuses the camera on the first player to play
        /// </summary>
        void FocusCamera()
        {
            StartCoroutine(ZoomCamera());
            StartCameraTransform(players[Turn].Seat);
        }

        /// <summary>
        /// reverts all camara settings to default
        /// </summary>
        void UnfocusCamera()
        {
            StartCoroutine(ZoomCamera());
            StartCameraTransform(-1);
        }

        /// <summary>
        /// starts coroutines to adjust the cameras
        /// </summary>
        void StartCameraTransform(int seat)
        {
            StartCoroutine(MoveCamera(seat));
            StartCoroutine(RotateCamera(seat));
            StartCoroutine(MoveBoardCamera(seat));
        }

        // camera coroutines
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
        /// move the camera using lerp
        /// </summary>
        IEnumerator MoveCamera(int seat)
        {
            Vector3 origin = Camera.main.transform.position;
            Vector3 destination = seat >= 0 ?
                seatTransforms[seat].position : Vector3.zero;

            destination.z = origin.z;

            if (origin != new Vector3(0f, 0f, origin.z))
            {
                betLabel.SetActive(false);
                DeactivateFocusControls();
            }

            if (destination != origin)
            {
                float startTime = Time.time;

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
            }
            else
                yield return new WaitForSeconds(CameraTransformDuration);

            if (origin == new Vector3(0f, 0f, origin.z))
                pTLabels.SetActive(true);

            if (seat >= 0)
                SetFocusUI();
            else
                pTLabels.SetActive(false);
        }

        /// <summary>
        /// rotate the camera using lerp 
        /// </summary>
        IEnumerator RotateCamera(int seat)
        {
            float origin = Camera.main.transform.eulerAngles.z switch
            {
                270f => -CameraAngles[1],
                330f => -CameraAngles[0],
                _ => Camera.main.transform.eulerAngles.z
            };
            float destination = seat switch
            {
                2 or 7 => -CameraAngles[0],
                3 => -CameraAngles[1],
                4 or 9 => CameraAngles[0],
                8 => CameraAngles[1],
                _ => 0f
            };

            if (destination == origin)
                yield break;

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


        /// <summary>
        /// move the board camera using lerp
        /// </summary>
        IEnumerator MoveBoardCamera(int seat)
        {
            Vector2 origin = boardCamera.rect.position;
            Vector2 destination = new(boardCamera.rect.x,
                seat < 4 || seat > 7 ?
                BoardCamaraTop : BoardCamaraBottom);

            if (destination == origin)
                yield break;

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

        // MoveCamera helper functions
        /// <summary>
        /// deactivates the focus controls while the camera moves
        /// </summary>
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
            float multiplier =
                players[Turn].Seat < 4 || players[Turn].Seat > 7 ? 1f : -1f;

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
                callButton.SetActive(true);
            }
            else
            {
                betInput.gameObject.SetActive(false);
                raiseButton.SetActive(false);
                callButton.SetActive(false);
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

        // state methods
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
                    FocusCamera();
                    break;
                case State.Flop:
                case State.Turn:
                case State.River:
                    if (AllInCount() == Player.Count)
                    {
                        SkipToShowdown();
                        break;
                    }
                    SetTurn();
                    StartCoroutine(FlipBoard());
                    break;
                case State.Showdown:
                    StartCoroutine(FlipHoles());
                    UnfocusCamera();
                    break;
                default:
                    RoundEnd();
                    break;
            }
        }

        // round start methods
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

            SetActivePlayers();
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

            ResetActionsTaken();
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

        // deal methods
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

        // betting methods
        /// <summary>
        /// places a players bet and updates the UI
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bet"></param>
        void PlaceBet(PlayerScript player, int bet)
        {
            player.PlaceBet(bet);
            chipDisplays[player.Seat].text = player.Chips.ToString();

            UpdatePot(bet);
            if (player.Bet > TopBet)
                UpdateTopBet(player.Bet);

            IncrementActionsTaken();
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

        // flop, river, turn methods
        int AllInCount()
        {
            int allInCount = 0;

            for (int index = 0; index < Player.Count; index++)
            {
                if (players[index].Chips == 0)
                    allInCount++;
            }

            return allInCount;
        }

        void SkipToShowdown()
        {
            StartCoroutine(FlipBoardEarly());
            Game.SkipToShowdown();
            NextState();
        }

        IEnumerator FlipBoardEarly()
        {
            int index = State switch
            {
                State.River => RIVER,
                State.Turn => TURN,
                _ => 0
            };
            float waitTime = CameraTransformDuration / (index == 0 ? 5f : 3f);

            for (; index <= RIVER; index++)
            {
                yield return new WaitForSeconds(waitTime);
                board[index].FlipCard();
            }
        }

        void SetTurn()
        {
            ResetActionsTaken();
            Game.SetTurn();
            NextTurn();
        }

        IEnumerator FlipBoard()
        {
            float waitTime = CameraTransformDuration / 3f;

            switch (State)
            {
                case State.Flop:
                    for (int index = 0; index <= FLOP; index++)
                    {
                        yield return new WaitForSeconds(waitTime);
                        board[index].FlipCard();
                    }
                    break;
                case State.Turn:
                    yield return new WaitForSeconds(waitTime);
                    board[TURN].FlipCard();
                    break;
                case State.River:
                    yield return new WaitForSeconds(waitTime);
                    board[RIVER].FlipCard();
                    break;
            }
        }

        // showdown methods
        /// <summary>
        /// flips all the players' cards on the table
        /// </summary>
        IEnumerator FlipHoles()
        {
            float waitTime = CameraTransformDuration / ActivePlayers;
            for (int index = 0; index < Player.Count; index++)
            {
                if (players[index].Hand == Hand.Fold)
                    continue;

                yield return new WaitForSeconds(waitTime / 2f);
                players[index].FlipCard(1);
                yield return new WaitForSeconds(waitTime / 2f);
                players[index].FlipCard(0);
            }

            DetermineWinner();
        }

        /// <summary>
        /// finds the best hand among the players and settles the pot
        /// </summary>
        void DetermineWinner()
        {
            Hand bestHand;
            Rank highCard, kicker;
            int potDivision = 0;
            string winners = "Winners: ";

            players[0].SetHand(board);

            bestHand = players[0].Hand;
            highCard = players[0].HighCard;
            kicker = players[0].Kicker;

            for (int index = 1; index < Player.Count; index++)
            {
                players[index].SetHand(board);

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

            for (int index = 0; index < Player.Count; index++)
            {
                if (players[index].Hand == bestHand &&
                    players[index].HighCard == highCard &&
                    players[index].Kicker == kicker)
                {
                    players[index].HasBestHand = true;
                    potDivision++;
                }
            }

            for (int index = 0; index < Player.Count; index++)
            {
                if (!players[index].HasBestHand)
                    continue;

                players[index].Payout(Pot / potDivision);
                chipDisplays[players[index].Seat].text = players[index].Chips.ToString();
                winners += $"\n{players[index].Name}";
            }

            ReducePot(Pot / potDivision * potDivision);
            potDisplay.text = Pot.ToString(); 

            hand.text = $"Winning Hand:\n{bestHand.GetName()}";
            this.highCard.text = $"High Card:\n{highCard}";
            this.kicker.text = $"Kicker:\n{kicker}";
            this.winners.text = winners;

            winDisplay.SetActive(true);
        }

        void NextTurn()
        {
            if (ActivePlayers == 1)
            {
                SkipToShowdown();
                return;
            }

            if (ActionsTaken >= ActivePlayers)
            {
                bool isBettingRoundOver = true;

                for (int index = 0; index < Player.Count; index++)
                {
                    if (players[index].Bet < TopBet &&
                        players[index].Chips > 0 &&
                        players[index].Hand > Hand.Fold)
                    {
                        isBettingRoundOver = false;
                        break;
                    }
                }

                if (isBettingRoundOver)
                {
                    NextState();
                    return;
                }
            }

            Game.NextTurn();

            if (players[Turn].Chips == 0)
                IncrementActionsTaken();

            if (players[Turn].Chips == 0 || players[Turn].Hand == Hand.Fold)
            {
                NextTurn();
                return;
            }

            StartCameraTransform(players[Turn].Seat);
        }

        /// <summary>
        /// resets gameState to RoundStart
        /// </summary>
        void RoundEnd()
        {
            Discard();

            foreach (Card card in board)
                card.Discard();

            board = new Card[BOARD_SIZE];

            winDisplay.SetActive(false);

            hand.text = string.Empty;
            highCard.text = string.Empty;
            kicker.text = string.Empty;
            winners.text = string.Empty;

            for (int index = 0; index < Player.Count; index++)
            {
                if (players[index] == null)
                    break;

                players[index].ResetHand();

                if (players[index].Chips == 0)
                    foreach (Button button in addLeaveControls[players[index].Seat].GetComponentsInChildren<Button>())
                        if (button.gameObject.name == "Leave Table")
                        {
                            button.onClick.Invoke();
                            index--;
                            break;
                        }
            }

            TopBet = 0;

            foreach (GameObject control in addLeaveControls)
                control.SetActive(true);

            roundStart.SetActive(true);

            NextRound();
            HasRoundStarted = false;
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

            for (; index < Player.Count - 1; index++)
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

            if (bet < minimumBet)
                bet = minimumBet;

            bet -= players[Turn].Bet;

            if (bet >= players[Turn].Chips)
            {
                AllIn();
                return;
            }
            
            PlaceBet(players[Turn], bet);

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
            DecrementActivePlayers();
            NextTurn();
        }

        // Check event
        public void Check()
        {
            IncrementActionsTaken();
            NextTurn();
        }

        // Toggle Cards event
        /// <summary>
        /// flips player cards until button is released
        /// </summary>
        public void ToggleCards()
        {
            if (players[Turn].Password == string.Empty ||
                passwordInput.text == players[Turn].Password)
                players[Turn].FlipCards();
        }
    }
}
