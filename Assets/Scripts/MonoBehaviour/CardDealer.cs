using UnityEngine;
using static GameManager;

public class CardDealer : MonoBehaviour
{
    // non-static members
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject[] players;
    [SerializeField] GameObject board;
    Transform[][] playerTargets = new Transform[MAX_PLAYERS][];
    Transform[] boardTargets = new Transform[BOARD_SIZE];

    // unity messages
    void Start()
    {
        for (int index = 0; index < MAX_PLAYERS; index++)
            SetTargets(ref playerTargets[index], players[index]);

        SetTargets(ref boardTargets, board);
    }

    // non-static methods
    public void InstantiateCard(int playerIndex, int transformIndex)
    {
        Instantiate(cardPrefab, playerTargets[playerIndex][transformIndex]);
    }

    public void InstantiateCard(int index)
    {
        Instantiate(cardPrefab, boardTargets[index]);
    }

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
