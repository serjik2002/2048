using UnityEngine;

public class FpsUnlocker : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 120;
    }
}
