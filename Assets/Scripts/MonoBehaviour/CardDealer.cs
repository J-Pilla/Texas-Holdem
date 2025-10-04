using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class CardDealer : MonoBehaviour
{
    Deck deck = new Deck();
    Card[] cards;
    public Card[] Cards { get { return cards; } }
    InputAction flip;
    bool isFlipped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        cards = new Card[HAND_SIZE];
        for (int index = 0; index < 3; index++)
            deck.Shuffle();

        for (int index = 0;index < HAND_SIZE; index++)
            cards[index] = new Card(deck.CardIds[index], index < Hole.SIZE);

        cards[0] = new Card(Rank.Jack, Suit.Spades, true);
        cards[1] = new Card(Rank.Five, Suit.Hearts, true);
        cards[2] = new Card(Rank.Seven, Suit.Clubs, false);
        cards[3] = new Card(Rank.Ace, Suit.Hearts, false);
        cards[4] = new Card(Rank.Six, Suit.Hearts, false);
        cards[5] = new Card(Rank.King, Suit.Clubs, false);
        cards[6] = new Card(Rank.Four, Suit.Clubs, false);


        /*
           "id": 217,
            "hole": [
                "Jack of Spades",
                "Five of Hearts",
            ],
            "board": [
                "Seven of Clubs",
                "Ace of Hearts",
                "Six of Hearts",
                "King of Clubs",
                "Four of Clubs",
            ],
            "hand": "Straight",
            "highCard": "Five",
            "kicker": "Jack"
        */

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        flip = InputSystem.actions.FindAction("Jump");
    }


    // Update is called once per frame
    void Update()
    {
        if (flip.WasPressedThisFrame())
        {
            if (isFlipped)
            {
                for (int index = 0; index < 3; index++)
                    deck.Shuffle();

                for (int index = 0; index < HAND_SIZE; index++)
                    cards[index] = new Card(deck.CardIds[index], index < Hole.SIZE);
            }

            isFlipped = !isFlipped;
        }
    }
}
