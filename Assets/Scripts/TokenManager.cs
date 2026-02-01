using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TokenManager : MonoBehaviour
{
    [SerializeField] private float gridPaddingRow = 1.0f;
    [SerializeField] private float gridPaddingColumn = 1.0f;
    [SerializeField] private Token tokenPrefab;
    [SerializeField] private List<TokenMaterialData> tokenMaterialDatas;

    [SerializeField] private int gridRow = 3;
    [SerializeField] private int gridColumn = 3;

    [Header("Duplicate ID Settings")]
    [SerializeField] private bool allowDuplicateIds = true;
    [SerializeField] private int maxDuplicatedIds = 3; // how many different IDs can be duplicated
    [SerializeField] private int minInstancesPerDuplicatedId = 2; // minimum instances for a duplicated ID
    [SerializeField] private int maxInstancesPerDuplicatedId = 3; // maximum instances for a duplicated ID

    private Dictionary<Vector2Int, Token> tokenOccupiedPositions = new Dictionary<Vector2Int, Token>();

    public Token selectedToken;
    public int GridRow => gridRow;
    public int GridColumn => gridColumn;
    public List<TokenMaterialData> TokenMaterialDatas => tokenMaterialDatas;
    public Action onTokenChanged;

    private void Awake()
    {
        SetupAllToken();
    }


    public TokenCombination GetDefaultTokenCombination()
    {
        TokenCombination combination = new TokenCombination();
        foreach (var kvp in tokenOccupiedPositions)
        {
            combination.tokensInCombination.Add(new TokenData
            {
                tokenId = kvp.Value.TokenId,
                gridPosition = kvp.Value.GridPosition,
                isFlipped = kvp.Value.IsFlipped
            });
        }
        return combination;
    }

    private void SetupAllToken()
    {
        tokenOccupiedPositions.Clear();

        List<int> assignments = GenerateTokenAssignments();
        int assignIndex = 0;
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
                int tokenIdToAssign = assignments[assignIndex++];
                newToken.Setup(tokenIdToAssign, gridPos);
                newToken.MeshRenderer.material = tokenMaterialDatas.Find(data => data.tokenId == newToken.TokenId)?.tokenMaterial;
                tokenOccupiedPositions.Add(gridPos, newToken);
            }
        }

        onTokenChanged?.Invoke();
    }

    private List<int> GenerateTokenAssignments()
    {
        int totalSlots = gridRow * gridColumn;
        List<int> availableIds = new List<int>();
        foreach (var t in tokenMaterialDatas)
        {
            if (!availableIds.Contains(t.tokenId))
            {
                availableIds.Add(t.tokenId);
            }
        }

        List<int> assignments = new List<int>(totalSlots);

        if (availableIds.Count == 0)
        {
            for (int i = 0; i < totalSlots; i++) assignments.Add(i);
        }
        else if (allowDuplicateIds)
        {
            // Shuffle available IDs
            List<int> idsShuffled = new List<int>(availableIds);
            for (int i = 0; i < idsShuffled.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, idsShuffled.Count);
                int tmp = idsShuffled[i];
                idsShuffled[i] = idsShuffled[j];
                idsShuffled[j] = tmp;
            }

            int dupCount = Mathf.Min(maxDuplicatedIds, idsShuffled.Count);
            HashSet<int> dupIds = new HashSet<int>();
            for (int i = 0; i < dupCount; i++)
            {
                int remaining = totalSlots - assignments.Count;
                if (remaining <= 0) break;
                int id = idsShuffled[i];
                dupIds.Add(id);
                int count = UnityEngine.Random.Range(minInstancesPerDuplicatedId, maxInstancesPerDuplicatedId + 1);
                count = Mathf.Min(count, remaining);
                for (int c = 0; c < count; c++) assignments.Add(id);
            }

            // Fill remaining with singletons from non-duplicated ids
            foreach (var id in idsShuffled)
            {
                if (assignments.Count >= totalSlots) break;
                if (dupIds.Contains(id)) continue;
                assignments.Add(id);
            }

            // If still not full (not enough unique IDs), fill with any IDs
            int idx = 0;
            while (assignments.Count < totalSlots)
            {
                assignments.Add(idsShuffled[idx % idsShuffled.Count]);
                idx++;
            }
        }
        else
        {
            // No duplicates: cycle through available IDs
            int idx = 0;
            while (assignments.Count < totalSlots)
            {
                assignments.Add(availableIds[idx % availableIds.Count]);
                idx++;
            }
        }

        // Shuffle assignments to distribute randomly across grid positions
        for (int i = 0; i < assignments.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, assignments.Count);
            int tmp = assignments[i];
            assignments[i] = assignments[j];
            assignments[j] = tmp;
        }

        return assignments;
    }

    public void SetSelectedToken(Token token)
    {
        if (token == null)
        {
            foreach (var kvp in tokenOccupiedPositions)
            {
                kvp.Value.IsSelected = false;
            }
            selectedToken = null;
            return;
        }
        foreach (var kvp in tokenOccupiedPositions)
        {
            if (kvp.Value != token && kvp.Value.IsSelected)
            {
                kvp.Value.IsSelected = false;
            }
            else if (kvp.Value == token && !kvp.Value.IsSelected)
            {
                kvp.Value.IsSelected = true;
                selectedToken = kvp.Value;
            }
        }
    }

    public Token GetTokenAtPosition(Vector2Int gridPosition)
    {
        if (tokenOccupiedPositions.ContainsKey(gridPosition))
        {
            return tokenOccupiedPositions[gridPosition];
        }
        return null;
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

            foreach (var kvp in tokenOccupiedPositions)
            {
                kvp.Value.IsSelected = false;
            }

            selectedToken = null;

            onComplete?.Invoke();
            Debug.Log($"Swapped Token {tokenA.TokenId} with Token {tokenB.TokenId}");

            // Notify listeners (e.g., TokenReferenceManager) for validation
            onTokenChanged?.Invoke();
        });
    }
}

[Serializable]
public class TokenMaterialData
{
    public int tokenId;
    public Material tokenMaterial;
    public Sprite tokenSprite;
}