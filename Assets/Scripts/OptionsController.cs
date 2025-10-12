using UnityEngine;

public class OptionsController : MonoBehaviour
{
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private InputHandler _inputHandler;

    private void Start()
    {
        _optionsPanel.SetActive(false);
    }

    public void ToggleOptionsPanel()
    {
        _optionsPanel.SetActive(!_optionsPanel.activeSelf);
        _inputHandler.Active = !_inputHandler.Active;
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL("https://docs.google.com/document/d/1zeeJ2kKRbaKkHCSyXlcAdQ6rp8651tCChpjEkYAQgHU/edit?usp=sharing");
    }

    public void RateApp()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.uniteditforce.twentyfortyeight");
    }
}
