using UnityEngine;
using UnityEngine.UI;

namespace TexasHoldem.MonoScripts
{
    using static Game;

    /// <summary>
    /// MonoBehaviour in charge of instantiating cards visually
    /// </summary>
    public class ObjectManager : MonoBehaviour
    {
        // non-static members
        // fields
        [SerializeField] GameObject playerPrefab;
        [SerializeField] GameObject dealerButtonPrefab;
        [SerializeField] GameObject smallBlindButtonPrefab;
        [SerializeField] GameObject bigBlindButtonPrefab;
        [SerializeField] GameObject cardPrefab;
        [SerializeField] GameObject seats;
        [SerializeField] GameObject board;
        PlayerManager[] players = new PlayerManager[Player.MAX];
        int initializerIndex;
        GameObject dealerButton;
        GameObject smallBlindButton;
        GameObject bigBlindButton;
        Transform[] seatTargets = new Transform[Player.MAX];
        Transform[] boardTargets = new Transform[BOARD_SIZE];

        // unity messages
        void Start()
        {
            SetTargets(ref seatTargets, seats);
            SetTargets(ref boardTargets, board);
        }

        // methods
        /// <summary>
        /// instantiates the buttons
        /// </summary>
        /// <param name="index"></param>
        public void InstantiateButtons()
        {
            InstantiateButton(Blind.Dealer);
            InstantiateButton(Blind.Small);
            InstantiateButton(Blind.Big);
        }

        /// <summary>
        /// instantiates the button based on the Blind set.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="blind"></param>
        void InstantiateButton(Blind blind)
        {
            (int index, GameObject buttonPrefab) = blind switch
            {
                Blind.Small => (Player.SmallBlindIndex, smallBlindButtonPrefab),
                Blind.Big => (Player.BigBlindIndex, bigBlindButtonPrefab),
                _ => (Player.DealerIndex, dealerButtonPrefab)
            };
            GameObject button = Instantiate(buttonPrefab, seatTargets[index]);
            
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
        /// instantiates a player's card visually
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <param name="transformIndex"></param>
        public void InstantiatePlayerCard(int index, bool isFirstCard)
        {
            Vector3 offset = new(.2f, -.02f);
            GameObject card = Instantiate(cardPrefab, seatTargets[index]);
            card.transform.localPosition += isFirstCard ? offset : -offset;
        }

        /// <summary>
        /// instatiates a card on the board visually
        /// </summary>
        /// <param name="index"></param>
        public void InstantiateBoardCard(int index)
        {
            Instantiate(cardPrefab, boardTargets[index]);
        }

        /// <summary>
        /// destroys the dealerButton gameObject
        /// </summary>
        public void DestroyButtons()
        {
            Destroy(dealerButton);
            Destroy(smallBlindButton);
            Destroy(bigBlindButton);
        }

        /// <summary>
        /// find and sets positions for the cards to be instantiated visually
        /// </summary>
        /// <param name="targets"></param>
        /// <param name="gameObject"></param>
        void SetTargets(ref Transform[] targets, GameObject gameObject)
        {
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();

            targets = gameObject == seats ?
                new Transform[Player.MAX] : new Transform[BOARD_SIZE];

            for (int ctr = 0, index = 0; index < targets.Length; ctr++)
            {
                if (transforms[ctr].gameObject.CompareTag("Target"))
                {
                    targets[index] = transforms[ctr];
                    index++;
                }
            }
        }

        // event methods
        /// <summary>
        /// event called by "Add Player" buttons,
        /// instantiates a player game object
        /// </summary>
        /// <param name="seatIndex"></param>
        public void AddPlayer(int index)
        {
            players[index] = Instantiate(playerPrefab, seatTargets[index]).GetComponent<PlayerManager>();
        }

        /// <summary>
        /// event called by "Sit" buttons,
        /// initializes a C# Player object inside a Player Manager component,
        /// call AFTER SetInitializerIndex() event method
        /// </summary>
        /// <param name="name"></param>
        public void SitAtTable(TMPro.TMP_InputField inputField)
        {
            if (inputField.text != string.Empty)
                players[initializerIndex].InitializePlayer(inputField.text);
            else
                throw new System.Exception("Add text to input field");
        }

        /// <summary>
        /// event called by "Sit" buttons,
        /// assignes value to initilizerIndex,
        /// call BEFORE SitAtTable() event method
        /// </summary>
        /// <param name="index"></param>
        public void SetInitializerIndex(int index)
        {
            initializerIndex = index;
        }

        /// <summary>
        /// event called by "Leave Table" and "Cancel" buttons,
        /// destroys a player game object
        /// </summary>
        /// <param name="index"></param>
        public void LeaveTable(int index)
        {
            Player.DecrementCount();
            Destroy(players[index].gameObject);
        }
    }
}
