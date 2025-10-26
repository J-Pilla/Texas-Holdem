using UnityEngine;

namespace TexasHoldem.MonoScripts
{
    public class PlayerManager : MonoBehaviour
    {
        // non-static members
        // fields
        [SerializeField] Player player;
        
        // unity messages
        void Start()
        {
            InstantiateCards();
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
                GameObject card = Instantiate(player.Cards[index].CardPrefab, transform);
                card.transform.localPosition += index == 0 ? cardOffset : -cardOffset;
                card.GetComponent<SpriteRenderer>().sortingOrder = index == 0 ? 1 : 0;
            }
        }
    }
}
