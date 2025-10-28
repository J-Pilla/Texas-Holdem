using UnityEngine;

namespace TexasHoldem.MonoScripts
{
    using static Game;
    using static Player;

    /// <summary>
    /// class representing a player
    /// </summary>
    public class PlayerScript : MonoBehaviour
    {
        // field
        [SerializeField] GameObject cardPrefab;
        string _name;

        // properties
        public string Name
        {
            get { return _name; }
            set
            {
                if (value != string.Empty)
                    _name = value;
                else
                    throw new System.Exception("Add text to input field");
            }
        }
        public int Chips { get; private set; } = 500;
        public int Bet { get; set; } = 0;
        public bool HasBestHand { get; set; } = false;
        public Hand Hand { get; private set; } = Hand.NoPair;
        public Blind Blind { get; set; } = Blind.None;
        [field: SerializeField] public Hole Hole { get; set; } = new();
        public Card[] Cards { get { return Hole.Cards; } }
        public int CardCount { get { return Hole.CardCount; } }
        public Rank HighCard
        {
            get { return Hole.HighCard; }
            private set { Hole.HighCard = value; }
        }
        public Rank Kicker
        {
            get { return Hole.Kicker; }
            private set { Hole.Kicker = value; }
        }

        // unity messages
        void Awake()
        {
            IncrementCount();
        }

        // methods

        /// <summary>
        /// adds a Card to the Cards array
        /// </summary>
        /// <param name="cardId"></param>
        public void AddCard(int cardId, Transform parent)
        {
            Hole.AddCard(cardId, cardPrefab, parent);
        }

        /// <summary>
        /// removes Cards from Cards array
        /// </summary>
        public void Discard()
        {
            Hole.Discard();
        }

        /// <summary>
        /// run through algorithms to find the value of the hand
        /// </summary>
        /// <param name="board"></param>
        public void SetHand(Card[] board)
        {
            Card[] combined = new Card[HAND_SIZE]; // temporary "hand" with both the player's cards
            CombineCards(board, combined); // and the community cards

            SortCards(combined);

            if (Hand != Hand.Fold)
            {
                Hand = Hand.NoPair;
                CheckStraightFlush(combined);

                if (Hand < Hand.FourOAK)
                    CheckSets(combined);

                if (Hand < Hand.Flush)
                    CheckFlush(combined);

                if (Hand < Hand.Straight)
                    CheckStraight(combined);

                if (Hand == Hand.NoPair)
                    HighCard = Cards[0].Rank >= Cards[1].Rank ? Cards[0].Rank : Cards[1].Rank;

                if (HighCard > Rank.LowAce)
                    Kicker = HighCard == Cards[0].Rank ? Cards[1].Rank : Cards[0].Rank;
                else
                    Kicker = Rank.Ace == Cards[0].Rank ? Cards[1].Rank : Cards[0].Rank;
            }
        }

        /// <summary>
        /// combine community cards with players hand
        /// </summary>
        /// <param name="board"></param>
        /// <param name="combined"></param>
        void CombineCards(Card[] board, Card[] combined)
        {
            for (int count = 0; count < HAND_SIZE; count++)
            {
                combined[count] = count < Hole.SIZE ?
                    Cards[count] : board[count - Hole.SIZE];
            }
        }

        /// <summary>
        /// sort cards by rank
        /// </summary>
        /// <param name="cards"></param>
        void SortCards(Card[] cards)
        {
            for (int index1 = 0; index1 < HAND_SIZE - 1; index1++)
                for (int index2 = index1 + 1; index2 < HAND_SIZE; index2++)
                    if (cards[index1].Rank > cards[index2].Rank)
                        (cards[index1], cards[index2]) = (cards[index2], cards[index1]);
        }

        /// <summary>
        /// checks for a straight
        /// </summary>
        /// <param name="cards"></param>
        void CheckStraight(Card[] cards)
        {
            int length; // length of the straight

            for (int ctr = HAND_SIZE - 1, holeCount = 0; holeCount < 2; ctr--)
            {
                if (!cards[ctr].InPlayerHand)
                    continue; // if the card isn't in the player's hand, skip the iteration

                holeCount++;
                length = 1; // reset length to 1

                for (int index = ctr + 1; index < HAND_SIZE && length < 5; index++)
                {
                    if (cards[index].Rank - cards[index - 1].Rank > 1)
                        break; // break if the card isn't the next in the sequence for straights or a dupe

                    if (cards[index].Rank != cards[index - 1].Rank)
                        length++; // if the cards aren't equal, increse length
                }

                if (length < 5)
                {
                    int index = ctr - 1;
                    for (; index >= 0 && length < 5; index--)
                    {
                        if (cards[index + 1].Rank - cards[index].Rank > 1)
                            break; // break if the card isn't the next in the sequence for straights or a dupe

                        if (cards[index + 1].Rank != cards[index].Rank)
                            length++; // if the cards isn't a dupe, incraese length
                    }

                    if (length == 4 && // chacking for an ace - 5 straight
                        cards[index + 1].Rank == Rank.Two && // this is always the bottom card of the straight
                        cards[HAND_SIZE - 1].Rank == Rank.Ace)
                        length++; // if the bottom card of the straight is 2 and theres an ace, increse the length
                }

                if (length == 5)
                {
                    HighCard = cards[ctr].Rank;
                    Hand = Hand.Straight;
                    break;
                }
            }

            if (Hand < Hand.Straight)
            {
                for (int ctr = HAND_SIZE - 1; cards[ctr].Rank == Rank.Ace; ctr--)
                { // chacking for an ace - 5 straight
                    if (!cards[ctr].InPlayerHand)
                        continue; // if the card isn't in the player's hand, skip the iteration

                    length = 1;

                    for (int index = 0; index < HAND_SIZE && length < 5; index++)
                    {
                        if (cards[index].Rank - Rank.LowAce > length)
                            break; // break if the card isn't the next in the sequence for straights or a dupe

                        if (index > 0 && cards[index].Rank != cards[index - 1].Rank)
                            length++; // if the cards isn't a dupe, incraese length
                    }

                    if (length == 5)
                    {
                        HighCard = Rank.LowAce;
                        Hand = Hand.Straight;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// checks for a straight flush
        /// </summary>
        /// <param name="cards"></param>
        void CheckStraightFlush(Card[] cards)
        {
            int length; // length of the straight

            for (int ctr = HAND_SIZE - 1, holeCount = 0; holeCount < 2; ctr--)
            {
                if (!cards[ctr].InPlayerHand)
                    continue; // if the card isn't in the player's hand, skip the iteration

                holeCount++;
                length = 1; // reset length to 1
                Rank highCard = cards[ctr].Rank; // the highest card of the straight

                for (int index = ctr + 1; index < HAND_SIZE && length < 5; index++)
                {
                    if (cards[index].Rank - cards[index - 1].Rank > 1)
                        break; // break if the card isn't the next in the sequence for straights or a dupe

                    if (cards[index].Suit == cards[ctr].Suit)
                    { // if the card matches suits, increase length and update the high card
                        length++;
                        highCard = cards[index].Rank;
                    }
                }

                if (length < 5)
                {
                    Rank lowCard = cards[ctr].Rank; // the lowest card of the straight
                    for (int index = ctr - 1; index >= 0 && length < 5; index--)
                    {
                        if (cards[index + 1].Rank - cards[index].Rank > 1)
                            break; // break if the card isn't the next in the sequence for straights or a dupe

                        if (cards[index].Suit == cards[ctr].Suit)
                        { // if the card matches suits, increase length and update the low card
                            length++;
                            lowCard = cards[index].Rank;
                        }
                    }

                    if (length == 4 && lowCard == Rank.Two)
                    { // chacking for an ace - 5 straight flush
                        for (int index = HAND_SIZE - 1; cards[index].Rank == Rank.Ace; index--)
                        {
                            if (cards[index].Suit == cards[ctr].Suit)
                            {
                                length++;
                                break;
                            }
                        }
                    }
                }

                if (length == 5)
                {
                    HighCard = cards[ctr].Rank;
                    Hand = highCard == Rank.Ace ? Hand.RoyalFlush : Hand.StraightFlush;
                    break;
                }
            }

            if (Hand < Hand.StraightFlush)
            { // chacking for an ace - 5 straight flush
                for (int ctr = HAND_SIZE - 1; cards[ctr].Rank == Rank.Ace; ctr--)
                {
                    if (!cards[ctr].InPlayerHand)
                        continue; // if the card isn't in the player's hand, skip the iteration

                    length = 1;

                    for (int index = 0; index < HAND_SIZE && length < 5; index++)
                    {
                        if (cards[index].Rank - Rank.LowAce > length)
                            break; // break if the card isn't the next in the sequence for straights or a dupe

                        if (cards[index].Suit == cards[ctr].Suit)
                            length++; // if the card matches suits, increase length
                    }

                    if (length == 5)
                    {
                        HighCard = Rank.LowAce;
                        Hand = Hand.StraightFlush;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// checks for sets (matching ranks)
        /// </summary>
        /// <param name="cards"></param>
        void CheckSets(Card[] cards)
        {
            int[] matchCounts = new int[Hole.SIZE]; // tallys matches for each card in your hand
            Rank[] matches = new Rank[Hole.SIZE]; // tracks the ranks of the matches
            int matchIndex = 0; // used to check cards in your hand

            for (int ctr = HAND_SIZE - 1; ctr >= 0; ctr--)
            {
                if (!cards[ctr].InPlayerHand)
                    continue; // if the card isn't in the player's hand, skip the iteration

                matchCounts[matchIndex] = 0;
                matches[matchIndex] = cards[ctr].Rank;

                for (int index = 0; index < HAND_SIZE; index++)
                {
                    if (index == ctr || cards[ctr].Rank != cards[index].Rank)
                        continue; // skip the iteration if it's the same card, or they don't match

                    matchCounts[matchIndex]++;

                    if (matchCounts[matchIndex] == 3)
                    { // if there's three matches it's four of a kind
                        HighCard = matches[matchIndex];
                        Hand = Hand.FourOAK;
                        break; // four is the strongest hand in this function, break
                    }
                }

                if (Hand == Hand.FourOAK)
                    break;

                matchIndex++; // can only increment once
            }

            if (matchCounts[0] > 0 || matchCounts[1] > 0 && Hand < Hand.FourOAK)
            {
                // set matchIndex to the higher count, or the higher rank
                matchIndex = (matchCounts[0] - matchCounts[1]) switch
                {
                    > 0 => 0,
                    0 => matches[0] > matches[1] ? 0 : 1,
                    < 0 => 1
                };

                HighCard = matches[matchIndex];

                // send the higher count to check for full house and two pair
                CheckComboSets(cards, matchCounts[matchIndex]);
            }
        }

        /// <summary>
        /// checks for a full house or two pair
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="setMatches"></param>
        void CheckComboSets(Card[] cards, int setMatches)
        {
            int[] matchCounts = new int[BOARD_SIZE]; // tallys matches for each possible comparison
            Rank[] matches = new Rank[BOARD_SIZE];
            int matchIndex = 0;

            for (int ctr = HAND_SIZE - 1; ctr >= 0; ctr--)
            {
                if (cards[ctr].Rank == HighCard)
                    continue; // if the card matches the rank you already have a match of, skip the iteration

                matchCounts[matchIndex] = 0;
                matches[matchIndex] = cards[ctr].Rank;

                for (int index = 0; index < HAND_SIZE; index++)
                {
                    if (index == ctr || matches[matchIndex] != cards[index].Rank)
                        continue; // skip the iteration if it's the same card, or they don't match

                    matchCounts[matchIndex]++;

                    if (setMatches + matchCounts[matchIndex] == 3)
                    { // any combination of 1 match and 2 matches will grant a full house
                        Hand = Hand.FullHouse;
                        break; // house is the strongest hand in this function, break
                    }
                }

                if (Hand == Hand.FullHouse)
                    break;

                matchIndex++; // only increments if you make it to the end
            }

            if (Hand < Hand.FullHouse)
            {
                switch (setMatches)
                { // check for pair or three of a kind
                    case 2:
                        Hand = Hand.ThreeOAK;
                        break;
                    case 1:
                        Hand = Hand.OnePair;

                        for (int index = 0; index < BOARD_SIZE; index++)
                        { // finally, check for two pairs
                            if (matchCounts[index] == 1)
                            {
                                matchIndex = index;
                                break;
                            }
                        }

                        if (matchIndex < BOARD_SIZE)
                            Hand = Hand.TwoPair;

                        break;
                }
            }
        }

        /// <summary>
        /// checks for a flush
        /// </summary>
        /// <param name="cards"></param>
        void CheckFlush(Card[] cards)
        {
            int matchCount; // tallys matching suits

            for (int ctr = HAND_SIZE - 1; ctr >= 0; ctr--)
            {
                if (!cards[ctr].InPlayerHand)
                    continue; // if the card isn't in the player's hand, skip the iteration

                matchCount = 0; // reset match count to 0

                for (int index = 0; index < HAND_SIZE; index++)
                {
                    if (index == ctr || cards[ctr].Suit != cards[index].Suit)
                        continue; // skip the iteration if it's the same card, or they don't match

                    matchCount++;

                    if (matchCount == 4)
                    { // if there's four matches it's a flush
                        HighCard = cards[ctr].Rank;
                        Hand = Hand.Flush;
                        break;
                    }
                }

                if (Hand == Hand.Flush)
                    break;
            }
        }
    }
}
