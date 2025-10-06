using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameBoardView view;

    private GamePlayModel model;

    void Awake()
    {
        model = new GamePlayModel(4);
    }

    private void Start()
    {
        view.model = model;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MakeMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) MakeMove(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) MakeMove(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) MakeMove(Vector2Int.down);
    }

    private void MakeMove(Vector2Int dir)
    {
        bool moved = model.Move(dir);
        if (moved)
        {
            // Используем анимированное обновление
            view.RefreshWithAnimation(true);
        }
    }

    public void OnRestartButton()
    {
        model.Reset();
        view.Refresh();
    }
}