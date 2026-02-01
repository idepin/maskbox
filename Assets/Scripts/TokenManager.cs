using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TokenManager : MonoBehaviour
{
    [SerializeField] private float gridPaddingRow = 1.0f;
    [SerializeField] private float gridPaddingColumn = 1.0f;
    [SerializeField] private Token tokenPrefab;
    [SerializeField] private int gridRow = 3;
    [SerializeField] private int gridColumn = 3;

    private Dictionary<Vector2Int, Token> tokenOccupiedPositions = new Dictionary<Vector2Int, Token>();

    private void Start()
    {
        SetupAllToken();
    }

    private void SetupAllToken()
    {
        for (int x = 0; x < gridColumn; x++)
        {
            for (int y = 0; y < gridRow; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Token newToken = Instantiate(tokenPrefab, transform);
                newToken.transform.localPosition = new Vector3(
                    x * gridPaddingColumn,
                    0,
                    y * gridPaddingRow
                );
                newToken.Setup(gridPos.x + gridPos.y * gridColumn, gridPos);
                tokenOccupiedPositions.Add(gridPos, newToken);
            }
        }
    }

    public void SetSelectedToken(Token token)
    {
        foreach (var kvp in tokenOccupiedPositions)
        {
            if (kvp.Value != token && kvp.Value.IsSelected)
            {
                kvp.Value.IsSelected = false;
            }
            else if (kvp.Value == token && !kvp.Value.IsSelected)
            {
                kvp.Value.IsSelected = true;
            }
        }
    }

    public void SwapTokens(Token tokenA, Token tokenB, Action onComplete = null)
    {
        Vector2Int posA = tokenA.GridPosition;
        Vector2Int posB = tokenB.GridPosition;

        tokenA.BeginSwap();
        tokenB.BeginSwap();

        Sequence seq = DOTween.Sequence();
        seq.Join(tokenA.transform.DOLocalMove(new Vector3(
            posB.x * gridPaddingColumn,
            0,
            posB.y * gridPaddingRow
        ), 0.4f));
        seq.Join(tokenB.transform.DOLocalMove(new Vector3(
            posA.x * gridPaddingColumn,
            0,
            posA.y * gridPaddingRow
        ), 0.4f));
        seq.OnComplete(() =>
        {
            tokenOccupiedPositions[posA] = tokenB;
            tokenOccupiedPositions[posB] = tokenA;

            tokenA.Setup(tokenA.TokenId, posB);
            tokenB.Setup(tokenB.TokenId, posA);

            tokenA.EndSwap();
            tokenB.EndSwap();

            onComplete?.Invoke();
            Debug.Log($"Swapped Token {tokenA.TokenId} with Token {tokenB.TokenId}");
        });
    }
}
