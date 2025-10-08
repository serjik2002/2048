using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TileView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private float moveSpeed = 1500f; // пиксели в секунду
    [SerializeField] private float scaleAnimDuration = 0.15f;

    private int value;
    private RectTransform rectTransform;
    private Coroutine currentAnimation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetValue(int newValue, bool animate = false)
    {
        value = newValue;

        if (value == 0)
        {
            // Плитка пустая - делаем невидимой через прозрачность
            valueText.text = "";
            background.color = new Color32(0, 0, 0, 0);
            valueText.color = new Color32(0, 0, 0, 0);
            // Не отключаем GameObject чтобы не прерывать анимации
        }
        else
        {
            valueText.text = value.ToString();
            // ВАЖНО: Всегда устанавливаем правильный цвет текста и фона
            Color32 tileColor = GetColor(value);
            valueText.color = GetColor(value);
            background.color = tileColor;

            if (animate)
            {
                // Плавное появление
                if (currentAnimation != null)
                    StopCoroutine(currentAnimation);
                currentAnimation = StartCoroutine(FadeInCoroutine(tileColor));
            }
            else
            {
                // Мгновенное отображение - устанавливаем полную непрозрачность
                background.color = tileColor;
                valueText.color = GetColor(value);
            }
        }
    }

    // Анимация движения
    public void AnimateMove(Vector2 targetPosition, bool isMerge, System.Action onComplete = null)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateMoveCoroutine(rectTransform.anchoredPosition, targetPosition, onComplete));
    }

    public IEnumerator AnimateMoveCoroutine(Vector2 fromPosition, Vector2 toPosition, System.Action onComplete = null)
    {
        Vector2 startPosition = fromPosition;
        float distance = Vector2.Distance(startPosition, toPosition);
        float duration = distance / moveSpeed;

        // Минимальная и максимальная длительность
        duration = Mathf.Clamp(duration, 0.05f, 0.3f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out cubic для плавности
            t = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, toPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = toPosition;
        currentAnimation = null;
        onComplete?.Invoke();
    }

    // Анимация pop при слиянии
    public void AnimateScalePop()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(ScaleAnimation());
    }

    // Анимация появления новой плитки
    // Тепер цей метод приймає значення нової плитки
    public void AnimateSpawn(int newValue)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SpawnCoroutine(newValue));
    }

    // Корутина також приймає значення
    private IEnumerator SpawnCoroutine(int newValue)
    {
        // Спочатку встановлюємо значення та масштаб 0
        value = newValue;

        // Тепер налаштовуємо вигляд (колір, текст).
        // Плитка ще невидима, бо її масштаб 0.
        if (value != 0)
        {
            transform.localScale = Vector3.zero;
            valueText.text = value.ToString();
            Color32 tileColor = GetColor(value);
            background.color = tileColor;
            valueText.color = GetColor(value);
        }
        else
        {
            valueText.text = "";
            background.color = new Color32(0, 0, 0, 0);
        }

        // Запускаємо анімацію масштабування від 0 до 1
        float elapsed = 0f;
        while (elapsed < scaleAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleAnimDuration;
            t = 1f - Mathf.Pow(1f - t, 2f);

            float scale = Mathf.Lerp(0f, 1f, t);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }

        transform.localScale = Vector3.one;
        currentAnimation = null;
    }

    // Анимация "pop" при слиянии
    private IEnumerator ScaleAnimation()
    {
        float elapsed = 0f;
        float halfDuration = scaleAnimDuration / 2f;

        // Увеличиваем
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, t);
            yield return null;
        }

        elapsed = 0f;
        // Уменьшаем обратно
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t);
            yield return null;
        }

        transform.localScale = Vector3.one;
        currentAnimation = null;
    }

    private IEnumerator FadeInCoroutine(Color32 targetColor)
    {
        Color32 startColor = new Color32(targetColor.r, targetColor.g, targetColor.b, 0);
        Color32 targetTextColor = GetColor(value);
        Color32 startTextColor = new Color32(targetTextColor.r, targetTextColor.g, targetTextColor.b, 0);

        float elapsed = 0f;
        while (elapsed < scaleAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleAnimDuration;

            // Анимируем прозрачность фона и текста
            background.color = Color.Lerp(startColor, targetColor, t);
            valueText.color = Color.Lerp(startTextColor, targetTextColor, t);
            yield return null;
        }

        background.color = targetColor;
        valueText.color = targetTextColor;
        currentAnimation = null;
    }

    private Color32 GetColor(int val)
    {
        switch (val)
        {
            case 2: return new Color32(0, 255, 255, 255);   // Яскравий Блакитний (Cyan)
            case 4: return new Color32(128, 255, 0, 255);   // Неоновий Зелений
            case 8: return new Color32(0, 255, 128, 255);   // Весняний Зелений
            case 16: return new Color32(255, 255, 0, 255);   // Жовтий
            case 32: return new Color32(255, 150, 0, 255);   // Помаранчевий
            case 64: return new Color32(255, 100, 0, 255);   // Червоно-помаранчевий
            case 128: return new Color32(255, 0, 0, 255);       // Червоний
            case 256: return new Color32(255, 0, 100, 255);   // Малиновий
            case 512: return new Color32(170, 0, 255, 255);   // Пурпурний
            case 1024: return new Color32(80, 80, 255, 255);    // Глибокий Синій
            case 2048: return new Color32(255, 255, 255, 255); // Сяючий Білий
            default: return new Color32(30, 30, 60, 255);      // Колір за замовчуванням
        }
    }
}