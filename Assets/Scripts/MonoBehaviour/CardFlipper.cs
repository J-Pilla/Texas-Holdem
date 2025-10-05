using UnityEngine;
using UnityEngine.InputSystem;

public class CardFlipper : MonoBehaviour
{
    // serialized members
    [SerializeField] GameManager m_gameManager;
    //[SerializeField] CardDealer m_cardDealer;
    // private members
    SpriteRenderer m_spriteRenderer;
    InputAction flip;
    int flipCount = 0;
    bool isFlipped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        flip = InputSystem.actions.FindAction("Jump");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (flip.WasPressedThisFrame())
        {/*
            if (!isFlipped)
            {
                //m_spriteRenderer.sprite = Resources.Load($"Playing Cards\\{m_cardDealer.Cards[m_cardIndex].File}", typeof(Sprite)) as Sprite;
                m_spriteRenderer.sortingOrder = m_spriteRenderer.sortingOrder == 0 ? 1 : 0;
                flipCount++;
            }
            else
            {
                m_spriteRenderer.sprite = Resources.Load($"Playing Cards\\card-back{flipCount % 4 + 1}", typeof(Sprite)) as Sprite;
                m_spriteRenderer.sortingOrder = m_spriteRenderer.sortingOrder == 0 ? 1 : 0;
            }*/

            isFlipped = !isFlipped;
        }
    }
}
