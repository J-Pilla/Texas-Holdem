using System.Text.RegularExpressions;
using static GameManager;

public class Player
{
    // static members & properties
    static int dealerIndex = 0;
    public static byte Count { get; private set; } = 0;
    public static int DealerIndex
    {
        get { return dealerIndex; }
        set {  dealerIndex = value < Count ? value : 0; }
    }
    
    // non-static members & properties
    public string Name { get; private set; }
    public int Chips { get; private set; }
    public int Bet { get; set; }
    public Blind Blind { get; set; }
    public Hole Hole { get; set; }
    public Hand Hand { get; private set; }
    public string FullHand
    {
        get
        {
            string fullHand = Hand.ToString();

            switch (Hand)
            {
                case Hand.NoPair:
                case Hand.OnePair:
                case Hand.TwoPair:
                case Hand.FullHouse:
                case Hand.StraightFlush:
                case Hand.RoyalFlush:
                    string pattern = @"([a-z])([A-Z])";
                    string replacement = "$1 $2";
                    fullHand = Regex.Replace(fullHand, pattern, replacement);
                    break;
                case Hand.ThreeOAK:
                case Hand.FourOAK:
                    fullHand = fullHand.Replace("OAK", " of a Kind");
                    break;
            }

            return fullHand;
        }
    }

    // static method
    public void NextDealer()
    {
        if (DealerIndex < Count)
            DealerIndex++;
        else
            DealerIndex = 0;
    }

    public void SetInitialDealer(int dealerIndex)
    {
        DealerIndex = dealerIndex;
    }

    // non-static methods
    public void AddCard(int cardId)
    {
        Hole.AddCard(cardId);
    }

    // combine community cards with players hand
    void CombineCards(Card[] board, Card[] combined)
    {
        for (int count = 0; count < HAND_SIZE; count++)
        {
            combined[count] = count < Hole.SIZE ?
                Hole.Cards[count] : board[count - Hole.SIZE];
        }
    }

    // sort cards by rank
    void SortCards(Card[] cards)
    {
        for (int index1 = 0; index1 < HAND_SIZE - 1; index1++)
        {
            for (int index2 = index1 + 1; index2 < HAND_SIZE; index2++)
            {
                if (cards[index1].Rank > cards[index2].Rank)
                {
                    Card tempCard = cards[index2];
                    cards[index2] = cards[index1];
                    cards[index1] = tempCard;
                }
            }
        }
    }

    // checks for a straight
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
                Hole.HighCard = cards[ctr].Rank;
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
                    Hole.HighCard = Rank.LowAce;
                    Hand = Hand.Straight;
                    break;
                }
            }
        }
    }

    // checks for a straight flush
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
                Hole.HighCard = cards[ctr].Rank;
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
                    Hole.HighCard = Rank.LowAce;
                    Hand = Hand.StraightFlush;
                    break;
                }
            }
        }
    }

    // checks for sets (matching ranks)
    void checkSets(Card[] cards)
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
                    Hole.HighCard = matches[matchIndex];
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
            // set matchIndex to the higher count, default 0
            matchIndex = matchCounts[0] >= matchCounts[1] ? 0 : 1;

            Hole.HighCard = matches[matchIndex];
            // send the higher count to check for full house and two pair
            CheckComboSets(cards, matchCounts[matchIndex]);
        }
    }

    // checks for a full house or two pair
    void CheckComboSets(Card[] cards, int setMatches)
    {
        int[] matchCounts = new int[BOARD_SIZE]; // tallys matches for each possible comparison
        Rank[] matches = new Rank[BOARD_SIZE];
        int matchIndex = 0;

        for (int ctr = HAND_SIZE - 1; ctr >= 0; ctr--)
        { // matchIndex is not used outside the loop so I made it local
            if (cards[ctr].Rank == Hole.HighCard)
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

    // checks for a flush
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
                    Hole.HighCard = cards[ctr].Rank;
                    Hand = Hand.Flush;
                    break;
                }
            }

            if (Hand == Hand.Flush)
                break;
        }
    }

    // run through algorithms to find the value of the hand
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
                checkSets(combined);

            if (Hand < Hand.Flush)
                CheckFlush(combined);

            if (Hand < Hand.Straight)
                CheckStraight(combined);

            if (Hand == Hand.NoPair)
                Hole.HighCard = Hole.Cards[0].Rank >= Hole.Cards[1].Rank ? Hole.Cards[0].Rank : Hole.Cards[1].Rank;

            if (Hole.HighCard > Rank.LowAce)
                Hole.Kicker = Hole.HighCard == Hole.Cards[0].Rank ? Hole.Cards[1].Rank : Hole.Cards[0].Rank;
            else
                Hole.Kicker = Rank.Ace == Hole.Cards[0].Rank ? Hole.Cards[1].Rank : Hole.Cards[0].Rank;
        }
    }

    public Player(string name)
    {
        Count++;
        Name = name;
        Chips = 0;
        Hole = new Hole();
        Hand = Hand.NoPair;
    }
}
