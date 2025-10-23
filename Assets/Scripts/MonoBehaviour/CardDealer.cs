using UnityEngine;
using static GameManager;

/// <summary>
/// MonoBehaviour in charge of instantiating cards visually
/// </summary>
public class CardDealer : MonoBehaviour
{
    // non-static members
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject[] players;
    [SerializeField] GameObject board;
    readonly Transform[][] playerTargets = new Transform[Player.MAX][];
    Transform[] boardTargets = new Transform[BOARD_SIZE];

    // unity messages
    void Start()
    {
        for (int index = 0; index < Player.MAX; index++)
            SetTargets(ref playerTargets[index], players[index]);

        SetTargets(ref boardTargets, board);
    }

    // non-static methods
    /// <summary>
    /// instantiates a player's card visually
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="transformIndex"></param>
    public void InstantiatePlayerCard(int playerIndex, int transformIndex)
    {
        Instantiate(cardPrefab, playerTargets[playerIndex][transformIndex]);
    }

    /// <summary>
    /// instatiates a card on the board visually
    /// </summary>
    /// <param name="index"></param>
    public void InstantiateBoardCard(int index)
    {
        Instantiate(cardPrefab, boardTargets[index]);
    }

    /// <summary>
    /// find and sets positions for the cards to be instantiated visually
    /// </summary>
    /// <param name="targets"></param>
    /// <param name="gameObject"></param>
    void SetTargets(ref Transform[] targets, GameObject gameObject)
    {
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();

        targets = gameObject == board ?
            new Transform[BOARD_SIZE] : new Transform[Hole.SIZE];

        for (int ctr = 0, index = 0; index < targets.Length; ctr++)
        {
            if (transforms[ctr].gameObject != gameObject)
            {
                targets[index] = transforms[ctr];
                index++;
            }
        }
    }
}
