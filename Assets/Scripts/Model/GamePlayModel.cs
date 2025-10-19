using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class GamePlayModel
{
    public int Size { get; private set; } = 4;
    public int[,] Board { get; private set; }
    public int Score { get; private set; }
    public int MaxScore { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action GameOver;

    public class TileMove
    {
        public Vector2Int from;
        public Vector2Int to;
        public int value;
        public bool isMerge;
        public int mergedValue;
    }

    public Vector2Int? NewTilePosition { get; private set; } = null;
    public List<TileMove> LastMoves { get; private set; } = new List<TileMove>();
    private System.Random rng = new System.Random();

    private const string SAVE_KEY = "GameSaveData";

    public GamePlayModel(int size = 4)
    {
        Size = size;
        Board = new int[size, size];
        Load(); // 👈 спробуємо завантажити існуючу сесію
        if (IsEmpty())
        {
            Reset();
        }
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
        Save();
    }

    public bool Move(Vector2Int dir)
    {
        if (IsGameOver) return false;
        LastMoves.Clear();
        NewTilePosition = null;

        bool moved = false;
        bool[,] merged = new bool[Size, Size];

        int startRow = dir.y > 0 ? Size - 1 : 0;
        int endRow = dir.y > 0 ? -1 : Size;
        int stepRow = dir.y > 0 ? -1 : 1;

        int startCol = dir.x > 0 ? Size - 1 : 0;
        int endCol = dir.x > 0 ? -1 : Size;
        int stepCol = dir.x > 0 ? -1 : 1;

        List<TileMove> tempMoves = new List<TileMove>();

        for (int r = startRow; r != endRow; r += stepRow)
        {
            for (int c = startCol; c != endCol; c += stepCol)
            {
                if (Board[r, c] == 0) continue;

                int currentR = r;
                int currentC = c;
                Vector2Int startPos = new Vector2Int(r, c);
                int tileValue = Board[r, c];
                bool movedThisTile = false;
                Vector2Int finalPos = startPos;

                while (true)
                {
                    int nextR = currentR + dir.y;
                    int nextC = currentC + dir.x;

                    if (nextR < 0 || nextR >= Size || nextC < 0 || nextC >= Size)
                        break;

                    if (Board[nextR, nextC] == 0)
                    {
                        Board[nextR, nextC] = Board[currentR, currentC];
                        Board[currentR, currentC] = 0;
                        currentR = nextR;
                        currentC = nextC;
                        finalPos = new Vector2Int(currentR, currentC);
                        movedThisTile = true;
                        moved = true;
                    }
                    else if (Board[nextR, nextC] == Board[currentR, currentC] && !merged[nextR, nextC])
                    {
                        int newValue = Board[nextR, nextC] * 2;
                        Board[nextR, nextC] = newValue;
                        Board[currentR, currentC] = 0;
                        Score += newValue;
                        if (Score > MaxScore) MaxScore = Score; // 🔥 оновлюємо maxScore
                        merged[nextR, nextC] = true;
                        movedThisTile = true;
                        moved = true;

                        tempMoves.Add(new TileMove
                        {
                            from = startPos,
                            to = new Vector2Int(nextR, nextC),
                            value = tileValue,
                            isMerge = true,
                            mergedValue = newValue
                        });
                        break;
                    }
                    else break;
                }

                if (movedThisTile && finalPos != startPos)
                {
                    bool alreadyAddedAsMerge = tempMoves.Any(m => m.from == startPos && m.isMerge);

                    if (!alreadyAddedAsMerge)
                    {
                        tempMoves.Add(new TileMove
                        {
                            from = startPos,
                            to = finalPos,
                            value = tileValue,
                            isMerge = false
                        });
                    }
                }
            }
        }

        LastMoves.AddRange(tempMoves);

        if (moved)
        {
            SpawnTile();
            if (CheckGameOver()) IsGameOver = true;
            Save();
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
        NewTilePosition = pick;
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
        GameOver?.Invoke();
        return true;
    }

    private bool IsEmpty()
    {
        foreach (int v in Board)
            if (v != 0)
                return false;
        return true;
    }

    [Serializable]
    private class SaveData
    {
        public int size;
        public int[] flatBoard;
        public int score;
        public int maxScore;
        public bool gameOver;
    }

    public void Save()
    {
        SaveData data = new SaveData
        {
            size = Size,
            flatBoard = Flatten(Board),
            score = Score,
            maxScore = MaxScore,
            gameOver = IsGameOver
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;

        string json = PlayerPrefs.GetString(SAVE_KEY);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null) return;

        Size = data.size;
        Board = Unflatten(data.flatBoard, Size);
        Score = data.score;
        MaxScore = data.maxScore;
        IsGameOver = data.gameOver;
    }

    public void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
    }

    private int[] Flatten(int[,] board)
    {
        int[] flat = new int[Size * Size];
        int i = 0;
        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                flat[i++] = board[r, c];
        return flat;
    }

    private int[,] Unflatten(int[] flat, int size)
    {
        int[,] grid = new int[size, size];
        for (int i = 0; i < flat.Length; i++)
            grid[i / size, i % size] = flat[i];
        return grid;
    }
}
