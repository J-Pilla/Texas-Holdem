using UnityEngine;

namespace TexasHoldem
{
    /// <summary>
    /// class representing a card, uses CardIds
    /// from Deck.cs to initialize unique cards
    /// </summary>
    public class Card
    {
        // fields
        int id;

        // properties
        public GameObject CardObject { get; private set; }
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
            CardObject.transform.localPosition += index == 0 ? cardOffset : -cardOffset;
            CardObject.GetComponent<SpriteRenderer>().sortingOrder = index == 0 ? 0 : 1;
        }

        /// <summary>
        /// displays the sprite for the face of the card
        /// </summary>
        public void FlipCard()
        {
            SpriteRenderer spriteRenderer = CardObject.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.Load($"{File}", typeof(Sprite)) as Sprite;
            spriteRenderer.sortingOrder = spriteRenderer.sortingOrder == 0 ? 1 : 0;
        }

        /// <summary>
        /// destroys CardObject
        /// </summary>
        public void Discard()
        {
            GameObject.Destroy(CardObject);
        }

        // constructors
        public Card(int id, bool inPlayerHand, GameObject cardPrefab, Transform parent)
        {
            Initialize(id, inPlayerHand);

            CardObject = GameObject.Instantiate(cardPrefab, parent);
        }

        public Card(int id, bool inPlayerHand = false)
        {
            Initialize(id, inPlayerHand);
        }

        // constructor helper
        void Initialize(int id, bool inPlayerHand)
        {
            Id = id;
            Rank = (Rank)(Id % 13 + 2);
            Suit = (Suit)(Id % 4);
            InPlayerHand = inPlayerHand;
        }
        /// <summary>
        /// used to force cards for testing purposes
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="suit"></param>
        /// <param name="inPlayerHand"></param>
        [System.Obsolete]
        public Card(Rank rank, Suit suit, bool inPlayerHand)
        {
            Id += ((int)rank - 2) * ((int)suit + 1);
            Rank = rank;
            Suit = suit;
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
