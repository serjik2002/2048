using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class testcell : MonoBehaviour
{
    // � ������������ 'cell' � 'cellPrefab' ��� ������� �������
    public GameObject cellPrefab;

    void Start()
    {
        // 1. ��������� ����, ����� �������� 2048
        for (int i = 2; i <= 2048; i *= 2)
        {
            var cellObj = Instantiate(cellPrefab, transform);

            Image background = cellObj.GetComponent<Image>();
            // 3. ���������� TextMeshProUGUI
            TextMeshProUGUI text = cellObj.GetComponentInChildren<TextMeshProUGUI>();

            if (background != null && text != null)
            {
                Color tileColor = GetColor(i);
                background.color = tileColor;

                // 2. ��������� ����������� �������� ������
                text.text = i.ToString();


                text.color = GetColor(i);
            }
        }
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