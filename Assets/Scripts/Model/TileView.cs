using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TileView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private float moveSpeed = 2000f; // ������� � �������
    [SerializeField] private float scaleAnimDuration = 0.1f;

    private int value;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetValue(int newValue)
    {
        value = newValue;

        if (value == 0)
        {
            valueText.text = "";
            background.color = new Color32(0, 123, 183, 100);
        }
        else
        {
            valueText.text = value.ToString();
            valueText.color = GetColor(value);
            background.color = GetColor(value);
        }
    }

    // �������� � ������� ����������� (��� �������� ������)
    public void AnimateMoveWorld(Vector2 fromWorldPos, Vector2 toWorldPos, bool isMerge, System.Action onComplete = null)
    {
        StopAllCoroutines();
        StartCoroutine(MoveWorldCoroutine(fromWorldPos, toWorldPos, isMerge, onComplete));
    }

    private IEnumerator MoveWorldCoroutine(Vector2 fromWorldPos, Vector2 toWorldPos, bool isMerge, System.Action onComplete)
    {
        print("AnimateMoveWorld");
        rectTransform.position = fromWorldPos;

        float distance = Vector2.Distance(fromWorldPos, toWorldPos);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // ����� ������������ ease-out ��� ����� ��������� ��������
            t = 1f - Mathf.Pow(1f - t, 3f);
            rectTransform.position = Vector2.Lerp(fromWorldPos, toWorldPos, t);
            yield return null;
        }

        rectTransform.position = toWorldPos;

        // Pop �������� �� �������� ������ ��� �������
        if (isMerge)
        {
            yield return StartCoroutine(ScaleAnimation());
        }

        onComplete?.Invoke();
    }

    // ��������� ����� ��� �������� pop (���������� �� ��������� ������)
    public void AnimateScalePop()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleAnimation());
    }

    // �������� ��������� ����� ������
    public void AnimateSpawn()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        transform.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < scaleAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleAnimDuration;
            // Ease-out ��� ����� �������� ���������
            t = 1f - Mathf.Pow(1f - t, 2f);
            transform.localScale = Vector3.one * t;
            yield return null;
        }

        transform.localScale = Vector3.one;
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
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.15f, t);
            yield return null;
        }

        elapsed = 0f;
        // ��������� �������
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    private Color32 GetColor(int val)
    {
        switch (val)
        {
            case 2: return new Color32(0, 180, 255, 255);   // �����-��������� ����
            case 4: return new Color32(255, 150, 60, 255);  // ������������ ����
            case 8: return new Color32(255, 70, 180, 255);  // ������-���������� ����
            case 16: return new Color32(255, 50, 120, 255);  // �������� ���������
            case 32: return new Color32(0, 255, 180, 255);   // �����-������� ����
            case 64: return new Color32(0, 255, 255, 255);   // ��������
            case 128: return new Color32(255, 200, 50, 255);  // ������ ����
            case 256: return new Color32(255, 100, 255, 255); // ����������
            case 512: return new Color32(150, 100, 255, 255); // ������
            case 1024: return new Color32(80, 160, 255, 255);  // ���� ����
            case 2048: return new Color32(255, 255, 255, 255); // ���� � �������� ������
            default: return new Color32(30, 30, 60, 255);    // �����-���� ���
        }
    }

}