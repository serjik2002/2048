using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameBoardView view;
    [SerializeField] private RewardedController _rewardedPanel;
    [SerializeField] private InputHandler _inputHandler;

    private GamePlayModel model;


    [SerializeField] private int _rewardUndoValue = 3;

    void Awake()
    {
        model = new GamePlayModel(4);
    }

    private void Start()
    {
        _inputHandler.OnMoveInput += MakeMove;
        model.GameOver += view.GameOver;
        model.IsUndo += view.Refresh;
        model.UndoCountChanged += () => view.ChangeUndoCounter(model.UndoCount);
        view.model = model;
    }

    void Update()
    {

        // Отладочная перезагрузка
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRestartButton();
        }
    }



    private void MakeMove(Vector2Int dir)
    {
        if (view.IsAnimating)
        {
            return;
        }
        bool moved = model.Move(dir);
        if (moved)
        {
            // Используем анимированное обновление
            view.RefreshWithAnimation(true);
            AdManager.Instance.ShowInterstitialAd();
        }
    }

    public void OnRestartButton()
    {
        model.Reset();
        view.Refresh();
    }

    public void Reload()
    {
        model.Reset();
        view.Refresh();
        view.ChangeUndoCounter(model.UndoCount);
    }



    public void Undo()
    {
        if (!model.IsCanUndo) return;

        if (model.UndoCount > 0)
        {
            model.Undo();
        }
        else
        {
            _rewardedPanel.ToggleRewardedPanel();
        }
    }

    public void RestoreUndo()
    {
        model.RestoreUndo(_rewardUndoValue);
    }
}