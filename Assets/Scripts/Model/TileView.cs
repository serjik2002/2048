using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TileView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private float moveSpeed = 1500f; // ������� � �������
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
            // ������ ������ - ������ ��������� ����� ������������
            valueText.text = "";
            background.color = new Color32(0, 0, 0, 0);
            valueText.color = new Color32(0, 0, 0, 0);
            // �� ��������� GameObject ����� �� ��������� ��������
        }
        else
        {
            valueText.text = value.ToString();
            // �����: ������ ������������� ���������� ���� ������ � ����
            Color32 tileColor = GetColor(value);
            valueText.color = GetColor(value);
            background.color = tileColor;

            if (animate)
            {
                // ������� ���������
                if (currentAnimation != null)
                    StopCoroutine(currentAnimation);
                currentAnimation = StartCoroutine(FadeInCoroutine(tileColor));
            }
            else
            {
                // ���������� ����������� - ������������� ������ ��������������
                background.color = tileColor;
                valueText.color = GetColor(value);
            }
        }
    }

    // �������� ��������
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

        // ����������� � ������������ ������������
        duration = Mathf.Clamp(duration, 0.05f, 0.3f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out cubic ��� ���������
            t = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, toPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = toPosition;
        currentAnimation = null;
        onComplete?.Invoke();
    }

    // �������� pop ��� �������
    public void AnimateScalePop()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(ScaleAnimation());
    }

    // �������� ��������� ����� ������
    // ����� ��� ����� ������ �������� ���� ������
    public void AnimateSpawn(int newValue)
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SpawnCoroutine(newValue));
    }

    // �������� ����� ������ ��������
    private IEnumerator SpawnCoroutine(int newValue)
    {
        // �������� ������������ �������� �� ������� 0
        value = newValue;

        // ����� ����������� ������ (����, �����).
        // ������ �� ��������, �� �� ������� 0.
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

        // ��������� ������� ������������� �� 0 �� 1
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

    // �������� "pop" ��� �������
    private IEnumerator ScaleAnimation()
    {
        float elapsed = 0f;
        float halfDuration = scaleAnimDuration / 2f;

        // �����������
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, t);
            yield return null;
        }

        elapsed = 0f;
        // ��������� �������
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

            // ��������� ������������ ���� � ������
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
            case 2: return new Color32(0, 255, 255, 255);   // �������� ��������� (Cyan)
            case 4: return new Color32(128, 255, 0, 255);   // �������� �������
            case 8: return new Color32(0, 255, 128, 255);   // �������� �������
            case 16: return new Color32(255, 255, 0, 255);   // ������
            case 32: return new Color32(255, 150, 0, 255);   // ������������
            case 64: return new Color32(255, 100, 0, 255);   // �������-������������
            case 128: return new Color32(255, 0, 0, 255);       // ��������
            case 256: return new Color32(255, 0, 100, 255);   // ���������
            case 512: return new Color32(170, 0, 255, 255);   // ���������
            case 1024: return new Color32(80, 80, 255, 255);    // �������� ����
            case 2048: return new Color32(255, 255, 255, 255); // ������ �����
            default: return new Color32(30, 30, 60, 255);      // ���� �� �������������
        }
    }
}