using System;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayModel
{
    public int Size { get; private set; } = 4;
    public int[,] Board { get; private set; }
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    // Для анимации
    public class TileMove
    {
        public Vector2Int from;
        public Vector2Int to;
        public int value;
        public bool isMerge;
        public int mergedValue; // значение после слияния
    }
    public Vector2Int? NewTilePosition { get; private set; } = null; // Позиция новой плитки

    public List<TileMove> LastMoves { get; private set; } = new List<TileMove>();

    private System.Random rng = new System.Random();

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
        Array.Clear(Board, 0, Board.Length);
        LastMoves.Clear();
        NewTilePosition = null;
        SpawnTile();
        SpawnTile();
    }

    public bool Move(Vector2Int dir)
    {
        if (IsGameOver) return false;
        LastMoves.Clear();
        NewTilePosition = null; // Сбрасываем перед ходом

        bool moved = false;
        bool[,] merged = new bool[Size, Size];

        int startRow = dir.y > 0 ? Size - 1 : 0;
        int endRow = dir.y > 0 ? -1 : Size;
        int stepRow = dir.y > 0 ? -1 : 1;

        int startCol = dir.x > 0 ? Size - 1 : 0;
        int endCol = dir.x > 0 ? -1 : Size;
        int stepCol = dir.x > 0 ? -1 : 1;

        for (int r = startRow; r != endRow; r += stepRow)
        {
            for (int c = startCol; c != endCol; c += stepCol)
            {
                if (Board[r, c] == 0) continue;

                int cr = r, cc = c;
                Vector2Int startPos = new Vector2Int(r, c);
                int tileValue = Board[r, c];
                bool thisTileMoved = false;
                bool thisTileMerged = false;

                while (true)
                {
                    int nr = cr - dir.y;
                    int nc = cc + dir.x;

                    if (nr < 0 || nr >= Size || nc < 0 || nc >= Size)
                        break;

                    if (Board[nr, nc] == 0)
                    {
                        Board[nr, nc] = Board[cr, cc];
                        Board[cr, cc] = 0;
                        cr = nr; cc = nc;
                        thisTileMoved = true;
                        moved = true;
                    }
                    else if (Board[nr, nc] == Board[cr, cc] && !merged[nr, nc])
                    {
                        Board[nr, nc] *= 2;
                        Board[cr, cc] = 0;
                        Score += Board[nr, nc];
                        merged[nr, nc] = true;

                        // Записываем движение со слиянием
                        LastMoves.Add(new TileMove
                        {
                            from = startPos,
                            to = new Vector2Int(nr, nc),
                            value = tileValue,
                            isMerge = true,
                            mergedValue = Board[nr, nc]
                        });

                        thisTileMoved = true;
                        thisTileMerged = true;
                        moved = true;
                        break;
                    }
                    else
                        
                    // Если плитка сдвинулась БЕЗ слияния
                    if (thisTileMoved && !thisTileMerged)
                    {
                        LastMoves.Add(new TileMove
                        {
                            from = startPos,
                            to = new Vector2Int(cr, cc),
                            value = tileValue,
                            isMerge = false
                        });
                    }
                }

                
            }
        }

        if (moved)
        {
            SpawnTile();
            if (CheckGameOver()) IsGameOver = true;
        }
        return moved;
    }

    private void SpawnTile()
    {
        List<Vector2Int> empties = new List<Vector2Int>();
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (Board[r, c] == 0)
                    empties.Add(new Vector2Int(r, c));

        if (empties.Count == 0) return;

        Vector2Int pick = empties[rng.Next(empties.Count)];
        Board[pick.x, pick.y] = rng.NextDouble() < 0.9 ? 2 : 4;
        NewTilePosition = pick; // Запоминаем позицию новой плитки!
    }

    private bool CheckGameOver()
    {
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
            {
                if (Board[r, c] == 0) return false;
                if (r + 1 < Size && Board[r, c] == Board[r + 1, c]) return false;
                if (c + 1 < Size && Board[r, c] == Board[r, c + 1]) return false;
            }
        }
        return true;
    }

    public void PrintDebug()
    {
        string s = "";
        for (int r = 0; r < Size; r++)
        {
            for (int c = 0; c < Size; c++)
                s += Board[r, c].ToString().PadLeft(5);
            s += "\n";
        }
        Debug.Log(s + $"Score: {Score}");
    }
}