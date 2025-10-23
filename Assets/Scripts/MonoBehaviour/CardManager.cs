using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;
using static Deck;

/// <summary>
/// MonoBehaviour in charge of displaying the face of cards
/// </summary>
public class CardManager : MonoBehaviour
{
    // non-static members
    SpriteRenderer spriteRenderer;
    InputAction checkState;
    readonly string cardFile = new Card(CardIds[CardIndex]).File;
    readonly int orderInLayer = CardIndex < Player.Count ? 0 : 1;

    // unity messages
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        checkState = InputSystem.actions.FindAction("Jump");

        spriteRenderer.sortingLayerName = "Card";
        spriteRenderer.sortingOrder = orderInLayer;
    }

    private void LateUpdate()
    {
        if (checkState.WasPressedThisFrame())
            CheckState();
    }

    // non-static methods
    void CheckState()
    {
        switch(GameState)
        {
            case State.Flip:
                spriteRenderer.sprite = Resources.Load($"Playing Cards\\{cardFile}", typeof(Sprite)) as Sprite;
                spriteRenderer.sortingOrder = spriteRenderer.sortingOrder == 0 ? 1 : 0;
                break;
            case State.Reset:
                Destroy(gameObject);
                break;
        }
    }
}
