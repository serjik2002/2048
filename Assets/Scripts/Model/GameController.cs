using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameBoardView view;
    [SerializeField] private InputHandler _inputHandler;

    private GamePlayModel model;

    void Awake()
    {
        model = new GamePlayModel(4);
    }

    private void Start()
    {
        _inputHandler.OnMoveInput += MakeMove;
        model.GameOver += view.GameOver;
        view.model = model;
    }

    void Update()
    {

        // ���������� ������������
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
            // ���������� ������������� ����������
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
    }
}