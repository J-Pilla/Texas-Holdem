using UnityEngine;
using UnityEngine.InputSystem;

namespace TexasHoldem.MonoScripts
{
    using static Game;
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

        // unity messages
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            checkState = InputSystem.actions.FindAction("Jump");
        }

        void LateUpdate()
        {
            if (checkState.WasPressedThisFrame())
                CheckState();
        }

        // non-static methods
        /// <summary>
        /// checks the game's state and calls functions dependaning on the state
        /// </summary>
        void CheckState()
        {
            switch (GameManager.State)
            {
                case State.Flip:
                    FlipCard();
                    break;
                case State.NextRound:
                    Destroy(gameObject);
                    break;
            }
        }

        /// <summary>
        /// changes the appearance of the card to display the face
        /// </summary>
        public void FlipCard()
        {
            spriteRenderer.sprite = Resources.Load($"{cardFile}", typeof(Sprite)) as Sprite;
            spriteRenderer.sortingOrder = spriteRenderer.sortingOrder == 0 ? 1 : 0;
        }
    }
}
