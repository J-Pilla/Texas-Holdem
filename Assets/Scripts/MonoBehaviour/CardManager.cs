using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;
using static Deck;

public class CardManager : MonoBehaviour
{
    // private members
    SpriteRenderer m_spriteRenderer;
    InputAction checkState;
    string cardFile = new Card(CardIds[CardIndex]).File;
    int orderInLayer = CardIndex < Player.Count ? 0 : 1;

    private void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        checkState = InputSystem.actions.FindAction("Jump");

        m_spriteRenderer.sortingLayerName = "Card";
        m_spriteRenderer.sortingOrder = orderInLayer;
    }

    private void LateUpdate()
    {
        if (checkState.WasPressedThisFrame())
            CheckState();
    }

    void CheckState()
    {
        switch(GameState)
        {
            case State.Flip:
                m_spriteRenderer.sprite = Resources.Load($"Playing Cards\\{cardFile}", typeof(Sprite)) as Sprite;
                m_spriteRenderer.sortingOrder = m_spriteRenderer.sortingOrder == 0 ? 1 : 0;
                break;
            case State.Reset:
                Destroy(gameObject);
                break;
        }
    }
}
