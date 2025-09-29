using System.Collections.Generic;
using UnityEngine;

public struct TileMove
{
    public Vector2Int From;
    public Vector2Int To;
    public int Value;
    public bool IsMerge;
}

public class GamePlayModel
{
    public int Size { get; private set; }
    public int[,] Board { get; private set; }
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    System.Random rng = new System.Random();

    public GamePlayModel(int size = 4)
    {
        Size = size;
        Board = new int[size, size];
        Reset();
    }

    public void Reset()
    {
        Score = 0;
        IsGameOver = false;
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                Board[r, c] = 0;

        SpawnTile();
        SpawnTile();
    }

    void SpawnTile()
    {
        List<Vector2Int> empty = new();
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (Board[r, c] == 0)
                    empty.Add(new Vector2Int(r, c));

        if (empty.Count == 0) return;

        var pos = empty[rng.Next(empty.Count)];
        Board[pos.x, pos.y] = rng.NextDouble() < 0.9 ? 2 : 4;
    }

    public List<TileMove> Move(Vector2Int dir)
    {
        List<TileMove> moves = new();
        if (IsGameOver) return moves;

        bool[,] merged = new bool[Size, Size];
        bool moved = false;

        // порядок обходу
        int startRow = dir.y > 0 ? 0 : Size - 1;
        int endRow = dir.y > 0 ? Size : -1;
        int stepRow = dir.y > 0 ? 1 : -1;

        int startCol = dir.x > 0 ? 0 : Size - 1;
        int endCol = dir.x > 0 ? Size : -1;
        int stepCol = dir.x > 0 ? 1 : -1;

        for (int r = startRow; r != endRow; r += stepRow)
        {
            for (int c = startCol; c != endCol; c += stepCol)
            {
                if (Board[r, c] == 0) continue;

                int cr = r, cc = c;
                while (true)
                {
                    int nr = cr - dir.y; // інвертуємо вісь Y
                    int nc = cc + dir.x;

                    if (nr < 0 || nr >= Size || nc < 0 || nc >= Size)
                        break;

                    if (Board[nr, nc] == 0)
                    {
                        Board[nr, nc] = Board[cr, cc];
                        Board[cr, cc] = 0;

                        moves.Add(new TileMove { From = new(cr, cc), To = new(nr, nc), Value = Board[nr, nc], IsMerge = false });

                        cr = nr; cc = nc;
                        moved = true;
                    }
                    else if (Board[nr, nc] == Board[cr, cc] && !merged[nr, nc])
                    {
                        Board[nr, nc] *= 2;
                        Score += Board[nr, nc];
                        Board[cr, cc] = 0;
                        merged[nr, nc] = true;

                        moves.Add(new TileMove { From = new(cr, cc), To = new(nr, nc), Value = Board[nr, nc], IsMerge = true });

                        moved = true;
                        break;
                    }
                    else break;
                }
            }
        }

        if (moved)
        {
            SpawnTile();
            if (CheckGameOver()) IsGameOver = true;
        }

        return moves;
    }

    bool CheckGameOver()
    {
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (Board[r, c] == 0) return false;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            for (int r = 0; r < Size; r++)
                for (int c = 0; c < Size; c++)
                {
                    int nr = r - d.y;
                    int nc = c + d.x;
                    if (nr >= 0 && nr < Size && nc >= 0 && nc < Size)
                        if (Board[r, c] == Board[nr, nc]) return false;
                }
        }
        return true;
    }
}
