using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
    public event Action<Vector2Int> OnMoveInput; // Викликається при свайпі або натисканні клавіші

    [Header("Swipe Settings")]
    [SerializeField] private float minSwipeDistance = 50f; // Мінімальна довжина свайпу в пікселях
    public bool Active { get; set; } = true;

    private Vector2 startTouchPos;
    private Vector2 endTouchPos;
    private bool isSwiping = false;

    void Update()
    {
        if (!Active) return;
        // Загальна клавіатура (завжди доступна у збірках, де є клавіатура)
        HandleKeyboardInput();

        // Платформо-специфічні обробники:
#if UNITY_ANDROID || UNITY_IOS
        HandleTouchInput();
#endif

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
        HandleMouseInput();
#endif
    }

    // --- 1️⃣ Клавіатура (завжди) ---
    private void HandleKeyboardInput()
    {
        // Якщо потрібно перевести up/down як у твоєму проекті — роби заміну тут.
        if (Input.GetKeyDown(KeyCode.LeftArrow)) OnMoveInput?.Invoke(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) OnMoveInput?.Invoke(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) OnMoveInput?.Invoke(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.DownArrow)) OnMoveInput?.Invoke(Vector2Int.up);
    }

#if UNITY_ANDROID || UNITY_IOS
    // --- 2️⃣ Сенсор (тільки мобільні збірки) ---
    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                startTouchPos = touch.position;
                isSwiping = true;
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (!isSwiping) return;
                endTouchPos = touch.position;
                DetectSwipeDirection(endTouchPos - startTouchPos);
                isSwiping = false;
                break;
        }
    }
#endif

#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
    // --- 3️⃣ Миша (для редактора / ПК / WebGL) ---
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            isSwiping = true;
        }

        if (Input.GetMouseButtonUp(0) && isSwiping)
        {
            endTouchPos = Input.mousePosition;
            DetectSwipeDirection(endTouchPos - startTouchPos);
            isSwiping = false;
        }
    }
#endif

    // --- Основна логіка розпізнавання свайпу ---
    private void DetectSwipeDirection(Vector2 delta)
    {
        if (delta.magnitude < minSwipeDistance)
            return; // надто короткий свайп

        delta.Normalize();

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Горизонтальний свайп
            if (delta.x > 0)
                OnMoveInput?.Invoke(Vector2Int.right);
            else
                OnMoveInput?.Invoke(Vector2Int.left);
        }
        else
        {
            // Вертикальний свайп
            if (delta.y > 0)
                OnMoveInput?.Invoke(Vector2Int.down);
            else
                OnMoveInput?.Invoke(Vector2Int.up);
        }
    }
}
