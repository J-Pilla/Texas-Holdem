using UnityEngine;
using static GameManager;

/// <summary>
/// MonoBehaviour in charge of instantiating cards visually
/// </summary>
public class CardDealer : MonoBehaviour
{
    // non-static members
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject player;
    [SerializeField] GameObject board;
    Transform[] playerTargets = new Transform[Player.MAX];
    Transform[] boardTargets = new Transform[BOARD_SIZE];
    readonly float offset = .25f;

    // unity messages
    void Start()
    {
        SetTargets(ref playerTargets, player);
        SetTargets(ref boardTargets, board);
    }

    // non-static methods
    /// <summary>
    /// instantiates a player's card visually
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="transformIndex"></param>
    public void InstantiatePlayerCard(int index, bool isFirstCard)
    {
        /// formula for position so far
        /// position.x = offset - Rx
        /// eularAngles.z <= 180
        /// R <= 90
        /// position.y = 0 + Rx
        /// R > 90
        /// position.y = Xoffset - (R - 90)x
        /// eularAngles.z > 180
        /// R <= 90
        /// position.y = 0 - Rx
        /// R > 90
        /// position.y = -Xoffset + (R - 90)x
        /// where offset = .25, R = angle, and x = offset / 90 degrees
        Vector3 offset =
            new(isFirstCard ? this.offset : -this.offset,
            isFirstCard ? this.offset : -this.offset);

        float angle = playerTargets[index].rotation.eulerAngles.z <= 180 ?
            playerTargets[index].rotation.eulerAngles.z :
            360f - playerTargets[index].rotation.eulerAngles.z;

        Vector3 position;
        position.x = offset.x - angle * offset.x / 90f;
        if (playerTargets[index].rotation.eulerAngles.z <= 180)
        {
            position.y = angle <= 90 ?
                0f + angle * offset.y / 90f :
                offset.x - (angle - 90) * offset.y / 90f;
        }
        else
        {
            position.y = angle <= 90 ?
                0f - angle * offset.y / 90f :
                -offset.x + (angle - 90) * offset.y / 90f;
        }
        position.z = 0f;

        Instantiate(cardPrefab,
            playerTargets[index].position + position,
            playerTargets[index].rotation);
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

        targets = gameObject == player ?
            new Transform[Player.MAX] : new Transform[BOARD_SIZE];

        for (int ctr = 0, index = 0; index < targets.Length; ctr++)
        {
            if (transforms[ctr].gameObject.CompareTag("Target"))
            {
                targets[index] = transforms[ctr];
                index++;
            }
        }
    }
}
