using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameBoardView : MonoBehaviour
{
    public TileView tilePrefab;
    public RectTransform gridContainer;

    Dictionary<Vector2Int, TileView> activeTiles = new();
    float cellSize;

    void Awake()
    {
        var layout = gridContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        cellSize = layout.cellSize.x; // припускаємо квадратні клітинки
    }

    public void ResetBoard(int[,] board)
    {
        foreach (var kv in activeTiles) Destroy(kv.Value.gameObject);
        activeTiles.Clear();

        int size = board.GetLength(0);
        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (board[r, c] != 0)
                    CreateTile(new Vector2Int(r, c), board[r, c]);
            }
        }
    }

    void CreateTile(Vector2Int pos, int value)
    {
        TileView tile = Instantiate(tilePrefab, gridContainer);
        tile.Rect.anchoredPosition = PosToUI(pos);
        tile.SetValue(value);
        activeTiles[pos] = tile;
    }

    Vector2 PosToUI(Vector2Int pos)
    {
        return new Vector2(pos.y * cellSize, -pos.x * cellSize);
    }

    public void ApplyMoves(List<TileMove> moves)
    {
        foreach (var move in moves)
        {
            if (activeTiles.TryGetValue(move.From, out var tile))
            {
                Vector2 target = PosToUI(move.To);
                StartCoroutine(AnimateMove(tile, target, 0.15f, move));

                activeTiles.Remove(move.From);
                activeTiles[move.To] = tile;
            }
        }
    }

    IEnumerator AnimateMove(TileView tile, Vector2 targetPos, float duration, TileMove move)
    {
        Vector2 start = tile.Rect.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            tile.Rect.anchoredPosition = Vector2.Lerp(start, targetPos, t);
            yield return null;
        }
        tile.Rect.anchoredPosition = targetPos;

        if (move.IsMerge)
        {
            tile.SetValue(move.Value);
            yield return StartCoroutine(MergeEffect(tile));
        }
    }

    IEnumerator MergeEffect(TileView tile)
    {
        Vector3 start = Vector3.one;
        Vector3 peak = Vector3.one * 1.2f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            tile.transform.localScale = Vector3.Lerp(start, peak, t <= 0.5f ? t * 2f : (1f - t) * 2f);
            yield return null;
        }
        tile.transform.localScale = Vector3.one;
    }
}
