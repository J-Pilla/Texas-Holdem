using System.IO;
using UnityEngine;

namespace TexasHoldem
{
    using static GameManager;
    using static Deck;

    /// <summary>
    /// MonoBehaviour in charge of generating json files for
    /// batch tests of generating hands for Texas Holdem
    /// </summary>
    [System.Obsolete]
    public class FileGenerator : MonoBehaviour
    {
        // static members
        // constants
        const int ITERATIONS = 1000;
        const string FILE = "data.json";

        // non-static members
        // fields
        readonly Player player = new();
        readonly Card[] board = new Card[BOARD_SIZE];
        readonly string[] holeCards = new string[Hole.SIZE];
        readonly string[] boardCards = new string[BOARD_SIZE];
        string id, hand, highCard, kicker;


        // unity messages
        void Awake()
        {
            if (File.Exists(FILE))
                File.Delete(FILE);

            File.Create(FILE);
        }

        void Start()
        {
            for (int ctr = 1; ctr <= ITERATIONS; ctr++)
            {
                id = (ctr).ToString();

                Shuffle();

                for (; CardIndex < HAND_SIZE; NextCard())
                {
                    if (CardIndex < Hole.SIZE)
                    {
                        player.Cards[CardIndex] = new Card(CardIds[CardIndex], true);
                        holeCards[CardIndex] = player.Cards[CardIndex].Name;
                    }
                    else
                    {
                        board[CardIndex - Hole.SIZE] = new Card(CardIds[CardIndex], false);
                        boardCards[CardIndex - Hole.SIZE] = board[CardIndex - Hole.SIZE].Name;
                    }
                }

                player.SetHand(board);

                hand = player.FullHand;
                highCard = player.HighCard.ToString();
                kicker = player.Kicker.ToString();

                SerializeData();
            }

            Application.Quit();
        }

        // non-static methods
        /// <summary>
        /// serializes data into json format that isn't a single line per entry
        /// </summary>
        void SerializeData()
        {
            string json = $"{{\n\t\"id\": {id},\n\t\"hole\": [\n\t";

            for (int index = 0; index < Hole.SIZE; index++)
                json += $"\t\"{holeCards[index]}\"" + (index < Hole.SIZE ? "," : "") + "\n\t";

            json += "],\n\t\"board\": [\n\t";

            for (int index = 0; index < BOARD_SIZE; index++)
                json += $"\t\"{boardCards[index]}\"" + (index < BOARD_SIZE ? "," : "") + "\n\t";

            json += $"],\n\t\"hand\": \"{hand}\",\n\t\"highCard\": " +
                $"\"{highCard}\",\n\t\"kicker\": \"{kicker}\"\n}}\n";

            File.AppendAllText(FILE, json);
        }
    }
}
