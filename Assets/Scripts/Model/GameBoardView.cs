using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameBoardView : MonoBehaviour
{
    [Header("Model + Prefab")]
    public GamePlayModel model;
    public GameObject tilePrefab;
    public GameObject animatedTilePrefab; // префаб дл€ анимированных плиток

    [Header("UI Links")]
    public Transform gridContainer;     // GridLayoutGroup - статичные плитки
    public Transform animationLayer;    // слой дл€ анимированных плиток (без Layout Group)
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI messageText;

    private TileView[,] tiles;
    private bool isAnimating = false;

    void Start()
    {
        InitBoard();
        Refresh();
    }

    private void InitBoard()
    {
        tiles = new TileView[model.Size, model.Size];

        for (int r = 0; r < model.Size; r++)
        {
            for (int c = 0; c < model.Size; c++)
            {
                GameObject go = Instantiate(tilePrefab, gridContainer);
                TileView tv = go.GetComponent<TileView>();
                tiles[r, c] = tv;
            }
        }
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
        for (int r = 0; r < model.Size; r++)
        {
            for (int c = 0; c < model.Size; c++)
            {
                tiles[r, c].SetValue(model.Board[r, c]);
            }
        }

        UpdateUI();
    }

    private IEnumerator AnimateMoves()
    {
        isAnimating = true;

        // —оздаЄм временные анимированные плитки
        List<GameObject> animatedTiles = new List<GameObject>();
        int completedMoves = 0;

        foreach (var move in model.LastMoves)
        {
            // —оздаЄм анимированную копию плитки
            GameObject animTileGO = Instantiate(animatedTilePrefab != null ? animatedTilePrefab : tilePrefab, animationLayer);
            TileView animTile = animTileGO.GetComponent<TileView>();
            animTile.SetValue(move.value);

            animatedTiles.Add(animTileGO);

            // ѕолучаем позиции из RectTransform статичных плиток
            Vector2 fromWorldPos = tiles[move.from.x, move.from.y].GetComponent<RectTransform>().position;
            Vector2 toWorldPos = tiles[move.to.x, move.to.y].GetComponent<RectTransform>().position;

            RectTransform animRect = animTileGO.GetComponent<RectTransform>();
            animRect.position = fromWorldPos;

            // —крываем только исходную плитку
            tiles[move.from.x, move.from.y].SetValue(0);

            // ≈сли сли€ние - оставл€ем целевую плитку видимой со старым значением
            if (move.isMerge && move.from != move.to)
            {
                tiles[move.to.x, move.to.y].SetValue(move.value);
            }
            else if (move.from != move.to)
            {
                tiles[move.to.x, move.to.y].SetValue(0);
            }

            // «апускаем анимацию
            int capturedToX = move.to.x;
            int capturedToY = move.to.y;
            bool capturedIsMerge = move.isMerge;

            animTile.AnimateMoveWorld(fromWorldPos, toWorldPos, move.isMerge, () =>
            {
                completedMoves++;
            });
        }

        // ∆дЄм завершени€ всех анимаций
        while (completedMoves < model.LastMoves.Count)
        {
            yield return null;
        }

        // ”дал€ем анимированные плитки
        foreach (var tile in animatedTiles)
        {
            Destroy(tile);
        }

        // ќбновл€ем все плитки которые двигались (устанавливаем финальные значени€)
        foreach (var move in model.LastMoves)
        {
            tiles[move.to.x, move.to.y].SetValue(model.Board[move.to.x, move.to.y]);

            // Pop анимаци€ на статичной плитке при сли€нии
            if (move.isMerge)
            {
                tiles[move.to.x, move.to.y].AnimateScalePop();
            }
        }

        yield return new WaitForSeconds(0.05f);


        UpdateUI();
        isAnimating = false;
    }

    private void UpdateUI()
    {
        scoreText.text = model.Score.ToString();
        messageText.text = model.IsGameOver ? "Game Over!" : "";
    }
}