using System.IO;
using UnityEngine;
using static GameManager;
using static Deck;

public class FileGenerator : MonoBehaviour
{
    const int iterations = 1000;
    const string FILE = "data.json";
    Player player = new Player("Jason");
    Card[] board = new Card[BOARD_SIZE];
    string[] holeCards = new string[Hole.SIZE];
    string[] boardCards = new string [BOARD_SIZE];
    string id, hand, highCard, kicker;

    // Start ias called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (File.Exists(FILE))
            File.Delete(FILE);

        File.Create(FILE);
    }

    private void Start()
    {
        InitializeDeck();
        for (int i = 0; i < iterations; i++)
        {
            id = (i + 1).ToString();

            Shuffle();

            for (; CardIndex < HAND_SIZE; CardIndex++)
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
