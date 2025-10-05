using System.IO;
using UnityEngine;
using static GameManager;

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

        for (int i = 0; i < iterations; i++)
        {
            id = (i + 1).ToString();

            for (int index = 0; index < 3; index++)
                Deck.Shuffle();

            for (int index = 0; index < HAND_SIZE; index++)
            {
                if (index < Hole.SIZE)
                {
                    player.Hole.Cards[index] = new Card(Deck.CardIds[index], true);
                    holeCards[index] = player.Hole.Cards[index].Name;
                }
                else
                {
                    board[index - Hole.SIZE] = new Card(Deck.CardIds[index], false);
                    boardCards[index - Hole.SIZE] = board[index - Hole.SIZE].Name;
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
