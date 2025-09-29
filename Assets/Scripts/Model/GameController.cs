using UnityEngine;

public class GameController : MonoBehaviour
{
    GamePlayModel model;
    public GameBoardView view;

    private void Awake()
    {
        
        model = new GamePlayModel(4);
    }
    private void Start()
    {
        view.ResetBoard(model.Board);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) MakeMove(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) MakeMove(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) MakeMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) MakeMove(Vector2Int.right);
    }

   private void MakeMove(Vector2Int dir)
    {
        var moves = model.Move(dir);
        if (moves.Count > 0)
            view.ApplyMoves(moves);
    }
}
