using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameBoardView : MonoBehaviour
{
    [Header("Model + Prefabs")]
    public GamePlayModel model;
    public GameObject backgroundTilePrefab; // Пустая подложка
    public GameObject valueTilePrefab;      // Плитка со значением

    [Header("UI Links")]
    public RectTransform boardContainer;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScore;

    [Header("Board Settings")]
    [SerializeField] private float tileSize = 100f;
    [SerializeField] private float spacing = 10f;

    private GameObject[,] backgroundTiles; // Статичные подложки
    private TileView[,] valueTiles;        // Плитки со значениями
    private bool isAnimating = false;

    void Start()
    {
        InitBoard();
        Refresh();
    }

    private void InitBoard()
    {
        backgroundTiles = new GameObject[model.Size, model.Size];
        valueTiles = new TileView[model.Size, model.Size];

        float boardSize = model.Size * tileSize + (model.Size - 1) * spacing;
        float startX = -boardSize / 2f + tileSize / 2f;
        float startY = boardSize / 2f - tileSize / 2f;

        // Создаём фоновые подложки
        for (int r = 0; r < model.Size; r++)
        {
            for (int c = 0; c < model.Size; c++)
            {
                GameObject bgTile = Instantiate(backgroundTilePrefab, boardContainer);
                RectTransform rect = bgTile.GetComponent<RectTransform>();

                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(tileSize, tileSize);

                float x = startX + c * (tileSize + spacing);
                float y = startY - r * (tileSize + spacing);
                rect.anchoredPosition = new Vector2(x, y);

                backgroundTiles[r, c] = bgTile;
            }
        }

        // Создаём плитки со значениями (изначально все скрыты)
        for (int r = 0; r < model.Size; r++)
        {
            for (int c = 0; c < model.Size; c++)
            {
                GameObject valueTile = Instantiate(valueTilePrefab, boardContainer);
                RectTransform rect = valueTile.GetComponent<RectTransform>();

                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(tileSize, tileSize);

                float x = startX + c * (tileSize + spacing);
                float y = startY - r * (tileSize + spacing);
                rect.anchoredPosition = new Vector2(x, y);

                TileView tv = valueTile.GetComponent<TileView>();
                tv.SetValue(0); // Скрываем
                valueTiles[r, c] = tv;
            }
        }
    }

    // Получить позицию ячейки [r, c]
    private Vector2 GetCellPosition(int r, int c)
    {
        return backgroundTiles[r, c].GetComponent<RectTransform>().anchoredPosition;
    }

    public void Refresh()
    {
        RefreshWithAnimation(false);
    }

    public void RefreshWithAnimation(bool animate = true)
    {
        if (isAnimating) return;

        if (animate && model.LastMoves.Count > 0)
        {
            StartCoroutine(AnimateMoves());
        }
        else
        {
            RefreshImmediate();
        }
    }

    private void RefreshImmediate()
    {
        // Обновляем все плитки согласно модели БЕЗ анимации
        for (int r = 0; r < model.Size; r++)
        {
            for (int c = 0; c < model.Size; c++)
            {
                valueTiles[r, c].SetValue(model.Board[r, c], false); // false - без анимации
                valueTiles[r, c].GetComponent<RectTransform>().anchoredPosition = GetCellPosition(r, c);
            }
        }

        UpdateUI();
    }

    private IEnumerator AnimateMoves()
    {
        isAnimating = true;

        // Сначала скрываем все плитки, которые будут двигаться
        foreach (var move in model.LastMoves)
        {
            if (move.from != move.to)
            {
                valueTiles[move.from.x, move.from.y].SetValue(0, false);
            }
        }

        int completedMoves = 0;
        int totalMoves = model.LastMoves.Count;

        // Запускаем все анимации параллельно
        foreach (var move in model.LastMoves)
        {
            Vector2 fromPos = GetCellPosition(move.from.x, move.from.y);
            Vector2 toPos = GetCellPosition(move.to.x, move.to.y);

            TileView movingTile = valueTiles[move.from.x, move.from.y];

            // Устанавливаем начальную позицию и значение
            movingTile.GetComponent<RectTransform>().anchoredPosition = fromPos;

            if (move.isMerge)
            {
                movingTile.SetValue(move.value, false); // Показываем исходное значение
            }
            else
            {
                movingTile.SetValue(move.value, true); // Показываем финальное значение
            }

            // Анимируем движение
            StartCoroutine(AnimateSingleMove(movingTile, fromPos, toPos, move, () =>
            {
                completedMoves++;
            }));
        }

        // Ждём завершения всех анимаций
        while (completedMoves < totalMoves)
        {
            yield return null;
        }

        // Синхронизируем финальное состояние БЕЗ анимации для всех плиток
        for (int r = 0; r < model.Size; r++)
        {
            for (int c = 0; c < model.Size; c++)
            {
                valueTiles[r, c].SetValue(model.Board[r, c], false); // false - без анимации
                valueTiles[r, c].GetComponent<RectTransform>().anchoredPosition = GetCellPosition(r, c);
            }
        }

        yield return new WaitForSeconds(0.05f);

        // Анимируем появление только НОВОЙ плитки
        if (model.NewTilePosition.HasValue)
        {
            Vector2Int newPos = model.NewTilePosition.Value;
            valueTiles[newPos.x, newPos.y].SetValue(model.Board[newPos.x, newPos.y], true); // true - с анимацией
            valueTiles[newPos.x, newPos.y].AnimateSpawn();
        }

        for (int r = 0; r < model.Size; r++)
            for (int c = 0; c < model.Size; c++)
                if (model.Board[r, c] == 0)
                    valueTiles[r, c].SetValue(0, false);

        UpdateUI();
        isAnimating = false;
    }

    private IEnumerator AnimateSingleMove(TileView tile, Vector2 fromPos, Vector2 toPos, GamePlayModel.TileMove move, System.Action onComplete)
    {
        // Анимация движения
        yield return StartCoroutine(tile.AnimateMoveCoroutine(fromPos, toPos));

        // Если это слияние - анимируем scale pop
        if (move.isMerge)
        {
            // Обновляем значение на целевой плитке
            valueTiles[move.to.x, move.to.y].SetValue(move.mergedValue, false); // Без анимации fade
            valueTiles[move.to.x, move.to.y].AnimateScalePop();

            // Скрываем движущуюся плитку
            tile.SetValue(0, false);
        }

        onComplete?.Invoke();

        if (move.isMerge || move.value == 0)
        {
            tile.SetValue(0, false);
        }
    }

    private void UpdateUI()
    {
        scoreText.text = model.Score.ToString();
        bestScore.text = model.MaxScore.ToString();
    }
}