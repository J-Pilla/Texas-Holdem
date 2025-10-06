using UnityEngine;
using static GameManager;

public class CardDealer : MonoBehaviour
{
    // serialized members
    [SerializeField] GameObject m_cardPrefab;
    [SerializeField] GameObject[] m_players;
    [SerializeField] GameObject m_board;
    // private members
    Transform[][] playerTargets = new Transform[MAX_PLAYERS][];
    Transform[] boardTargets = new Transform[BOARD_SIZE];

    private void Start()
    {
        for (int index = 0; index < MAX_PLAYERS; index++)
            SetTargets(ref playerTargets[index], m_players[index]);

        SetTargets(ref boardTargets, m_board);
    }

    void SetTargets(ref Transform[] targets, GameObject gameObject)
    {
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();

        targets = gameObject == m_board ?
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

    public void InstantiateCard(int playerIndex, int transformIndex)
    {
        Instantiate(m_cardPrefab, playerTargets[playerIndex][transformIndex]);
    }

    public void InstantiateCard(int index)
    {
        Instantiate(m_cardPrefab, boardTargets[index]);
    }
}
