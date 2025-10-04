using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class HandChecker : MonoBehaviour
{
    [SerializeField] GUISkin m_guiSkin;
    [SerializeField] CardDealer m_cardDealer;
    Player player;
    Card[] board;
    InputAction flip;
    bool isFlipped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = new Player("Jason");
        board = new Card[BOARD_SIZE];
        flip = InputSystem.actions.FindAction("Jump");

        for (int index = 0; index < HAND_SIZE; index++)
        {
            if (index < Hole.SIZE)
                player.Hole.Cards[index] = m_cardDealer.Cards[index];
            else
                board[index - Hole.SIZE] = m_cardDealer.Cards[index];
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (flip.WasPressedThisFrame())
        {
            if (!isFlipped)
                player.SetHand(board);
            else
            {
                for (int index = 0; index < HAND_SIZE; index++)
                {
                    if (index < Hole.SIZE)
                        player.Hole.Cards[index] = m_cardDealer.Cards[index];
                    else
                        board[index - Hole.SIZE] = m_cardDealer.Cards[index];
                }
            }
            isFlipped = !isFlipped;
        }
    }

    private void OnGUI()
    {
        if (isFlipped)
            GUI.Label(new Rect(Screen.width / 2, Screen.height / 2 + 250, 0, 0),
                player.FullHand, m_guiSkin.label);
    }
}
