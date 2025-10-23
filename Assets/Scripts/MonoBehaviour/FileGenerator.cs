using System.IO;
using UnityEngine;
using static GameManager;
using static Deck;

/// <summary>
/// MonoBehaviour in charge of generating json files for
/// batch tests of generating hands for Texas Holdem
/// </summary>
[System.Obsolete]
public class FileGenerator : MonoBehaviour
{
    // non-static fields
    Player player = new Player("Jason");
    Card[] board = new Card[BOARD_SIZE];
    string[] holeCards = new string[Hole.SIZE];
    string[] boardCards = new string[BOARD_SIZE];
    string id, hand, highCard, kicker;

    // constants
    const int ITERATIONS = 1000;
    const string FILE = "data.json";

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
                    player.Hole.Cards[CardIndex] = new Card(CardIds[CardIndex], true);
                    holeCards[CardIndex] = player.Hole.Cards[CardIndex].Name;
                    print(CardIds[CardIndex].ToString());
                }
                else
                {
                    board[CardIndex - Hole.SIZE] = new Card(CardIds[CardIndex], false);
                    boardCards[CardIndex - Hole.SIZE] = board[CardIndex - Hole.SIZE].Name;
                    print(CardIndex.ToString());
                }
            }

            player.SetHand(board);

            hand = player.FullHand;
            highCard = player.Hole.HighCard.ToString();
            kicker = player.Hole.Kicker.ToString();

            SerializeData();
        }

        Application.Quit();
    }

    // non-static methods
    /// <summary>
    /// serializes data into a .json format that isn't a single line per entry
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
