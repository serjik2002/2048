using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileView : MonoBehaviour
{
    public TextMeshProUGUI ValueText;
    public RectTransform Rect;

    public void SetValue(int value)
    {
        ValueText.text = value.ToString();
    }

}
