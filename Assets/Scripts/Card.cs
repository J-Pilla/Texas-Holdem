using UnityEngine;

namespace TexasHoldem
{
    /// <summary>
    /// class representing a card, uses CardIds
    /// from Deck.cs to initialize unique cards
    /// </summary>
    public class Card
    {
        // static members
        // constants
        const string BackFile = "card-back1";

        // non-static members
        // fields
        int id;
        bool isFlipped = false;
        readonly GameObject cardObject;

        // properties
        public int Id
        {
            get { return id; }
            set
            {
                if (value >= 0 && value < Deck.SIZE)
                    id = value;
                else
                    id = 0;
            }
        }
        public Rank Rank { get; private set; }
        public Suit Suit { get; private set; }
        public bool InPlayerHand { get; private set; }
        public string Name { get { return $"{Rank} of {Suit}"; } }
        public string File
        {
            get
            {
                return $"card-{Suit}-{(int)(Rank == Rank.Ace ? Rank.LowAce : Rank)}".ToLower();
            }
        }

        // methods
        public override string ToString()
        {
            return $"Card: {Name}, Id: {Id}, Player Card: {InPlayerHand}";
        }

        /// <summary>
        /// sets the variation in position for the hole cards
        /// </summary>
        /// <param name="index"></param>
        public void OffSetPlayerCard(int index)
        {
            Vector3 cardOffset = new(-.2f, .02f);
            cardObject.transform.localPosition += index == 0 ? cardOffset : -cardOffset;
            cardObject.GetComponent<SpriteRenderer>().sortingOrder = index == 0 ? 0 : 1;
        }

        /// <summary>
        /// displays the sprite for the face of the card
        /// </summary>
        public void FlipCard()
        {
            SpriteRenderer spriteRenderer = cardObject.GetComponent<SpriteRenderer>();
            
            spriteRenderer.sprite = !isFlipped ?
                Resources.Load(File, typeof(Sprite)) as Sprite :
                Resources.Load(BackFile, typeof(Sprite)) as Sprite;
            spriteRenderer.sortingOrder = spriteRenderer.sortingOrder == 0 ? 1 : 0;

            isFlipped = !isFlipped;
        }

        /// <summary>
        /// destroys cardObject
        /// </summary>
        public void Discard()
        {
            GameObject.Destroy(cardObject);
        }

        // constructors
        public Card(int id, bool inPlayerHand, GameObject cardPrefab, Transform parent)
        {
            Initialize(id, inPlayerHand);

            cardObject = GameObject.Instantiate(cardPrefab, parent);
        }

        public Card(int id, bool inPlayerHand = false)
        {
            Initialize(id, inPlayerHand);
        }

        // constructor helper
        /// <summary>
        /// initialize the 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="inPlayerHand"></param>
        void Initialize(int id, bool inPlayerHand)
        {
            Id = id;
            Rank = (Rank)(Id % 13 + 2);
            Suit = (Suit)(Id % 4);
            InPlayerHand = inPlayerHand;
        }
    }

    /// <summary>
    /// card ranks
    /// </summary>
    public enum Rank
    {
        LowAce = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }

    /// <summary>
    /// card suits
    /// </summary>
    public enum Suit
    {
        Clubs,
        Diamonds,
        Hearts,
        Spades
    }
}
