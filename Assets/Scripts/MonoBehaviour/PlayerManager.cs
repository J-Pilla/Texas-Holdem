using UnityEngine;

namespace TexasHoldem.MonoScripts
{
    public class PlayerManager : MonoBehaviour
    {
        // non-static members
        // fields
        [SerializeField] Player player;
        GameObject[] cards = new GameObject[Hole.SIZE];

        // properties
        public Player Player { get { return player; } }
        
        // unity messages
        void Start()
        {

        }

        void Update()
        {

        }

        // methods
        /// <summary>
        /// initializes player object
        /// </summary>
        /// <param name="name"></param>
        public void InitializePlayer(string name)
        {
            player = new Player(name);
        }

        public void InstantiateCards()
        {
            Vector3 cardOffset = new(.2f, -.02f);
            for (int index = 0; index < Hole.SIZE; index++)
            {
                cards[index] = Instantiate(player.Cards[index].CardPrefab, transform);
                cards[index].transform.localPosition += index == 0 ? cardOffset : -cardOffset;
                cards[index].GetComponent<SpriteRenderer>().sortingOrder = index == 0 ? 1 : 0;
            }
        }

        public void DesroyCards()
        {
            foreach (GameObject card in cards)
                Destroy(card);
        }
    }
}
