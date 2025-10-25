using Unity.VisualScripting;
using UnityEngine;

namespace TexasHoldem
{
    using static GameManager;

    /// <summary>
    /// MonoBehaviour in charge of instantiating cards visually
    /// </summary>
    public class CardDealer : MonoBehaviour
    {
        // non-static members
        [SerializeField] GameObject dealerButtonPrefab;
        GameObject dealerButton;
        [SerializeField] GameObject smallBlindButtonPrefab;
        GameObject smallBlindButton;
        [SerializeField] GameObject bigBlindButtonPrefab;
        GameObject bigBlindButton;
        [SerializeField] GameObject cardPrefab;
        [SerializeField] GameObject seats;
        [SerializeField] GameObject board;
        Transform[] seatTargets = new Transform[Player.MAX];
        Transform[] boardTargets = new Transform[BOARD_SIZE];

        // unity messages
        void Start()
        {
            SetTargets(ref seatTargets, seats);
            SetTargets(ref boardTargets, board);
        }

        // non-static methods
        /// <summary>
        /// instantiates the dealer button visually
        /// </summary>
        /// <param name="index"></param>
        public void InstantiateButtons()
        {
            InstantiateButton(Player.DealerIndex);
            InstantiateButton(Player.SmallBlindIndex, Blind.Small);
            InstantiateButton(Player.BigBlindIndex, Blind.Big);
        }

        void InstantiateButton(int index, Blind blind = Blind.None)
        {
            GameObject buttonPrefab = blind switch
            {
                Blind.Small => smallBlindButtonPrefab,
                Blind.Big => bigBlindButtonPrefab,
                _ => dealerButtonPrefab
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
    }
}
