using UnityEngine;

public class RewardedController : MonoBehaviour
{
    [SerializeField] private GameObject _rewardePanel;
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private GameController _controller;

    private void Start()
    {
        _rewardePanel.SetActive(false);
    }

    public void ToggleRewardedPanel()
    {
        _rewardePanel.SetActive(!_rewardePanel.activeSelf);
        _inputHandler.Active = !_inputHandler.Active;
    }

    public void WatchRewarded()
    {
        AdManager.Instance.ShowRewardedAd((_) =>
        {
            _controller.RestoreUndo();
            ToggleRewardedPanel();
        });
    }

}
