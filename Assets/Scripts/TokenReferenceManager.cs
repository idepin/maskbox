using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenReferenceManager : MonoBehaviour
{
    [SerializeField] private TokenManager tokenManager;

    [SerializeField] private TokenCombination tokenCombination;
    [SerializeField] private TokenReference tokenReferencePrefab;
    [SerializeField] private TextMeshProUGUI pointText;
    public int currentPoint = 0;




    void Start()
    {
        var grid = GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.startCorner = GridLayoutGroup.Corner.LowerLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
        tokenCombination = tokenManager.GetDefaultTokenCombination();
        //SetupTokenReferences();
        tokenManager.onTokenChanged += ValidateToken;
        RandomizeTokenCombination();
    }

    void OnDestroy()
    {
        tokenManager.onTokenChanged -= ValidateToken;
    }

    private void ValidateToken()
    {
        // Validate current token combination against the stored one
        bool isValid = true;
        foreach (var tokenData in tokenCombination.tokensInCombination)
        {
            Token token = tokenManager.GetTokenAtPosition(tokenData.gridPosition);
            if (token == null)
            {
                Debug.LogWarning($"Validation failed: No token at {tokenData.gridPosition}");
                isValid = false;
                break;
            }
            if (token.TokenId != tokenData.tokenId)
            {
                Debug.LogWarning($"Validation failed: tokenId mismatch at {tokenData.gridPosition}. World {token.TokenId} vs UI {tokenData.tokenId}");
                isValid = false;
                break;
            }
            if (token.IsFlipped != tokenData.isFlipped)
            {
                Debug.LogWarning($"Validation failed: flip mismatch at {tokenData.gridPosition}. World {token.IsFlipped} vs UI {tokenData.isFlipped}");
                isValid = false;
                break;
            }
        }

        if (isValid)
        {
            Debug.Log("Token combination is valid!");
            currentPoint += 1;
            pointText.SetText(currentPoint.ToString());
        }
        else
        {
            Debug.Log("Token combination is invalid.");
        }
    }




    //RANDOMIZATION COMBINATION
    [ContextMenu("Randomize Token Combination")]
    private void RandomizeTokenCombination()
    {
        // Randomize to a unique permutation of all grid positions
        System.Random rand = new System.Random();

        List<Vector2Int> allPositions = new List<Vector2Int>();
        for (int x = 0; x < tokenManager.GridColumn; x++)
        {
            for (int y = 0; y < tokenManager.GridRow; y++)
            {
                allPositions.Add(new Vector2Int(x, y));
            }
        }

        // Fisher-Yates shuffle
        for (int i = allPositions.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            var tmp = allPositions[i];
            allPositions[i] = allPositions[j];
            allPositions[j] = tmp;
        }

        int count = Math.Min(tokenCombination.tokensInCombination.Count, allPositions.Count);
        for (int i = 0; i < count; i++)
        {
            var tokenData = tokenCombination.tokensInCombination[i];
            tokenData.gridPosition = allPositions[i];
            tokenData.isFlipped = rand.Next(0, 2) == 0;
        }

        RefreshTokenReferences();

        //Update TokenReferences

    }

    private void SetupTokenReferences()
    {
        // Match world grid visual order: bottom-to-top (y asc), then left-to-right (x asc)
        List<TokenData> sortedTokens = new List<TokenData>(tokenCombination.tokensInCombination);
        sortedTokens.Sort((a, b) =>
        {
            int yCmp = a.gridPosition.y.CompareTo(b.gridPosition.y);
            if (yCmp != 0) return yCmp;
            return a.gridPosition.x.CompareTo(b.gridPosition.x);
        });

        foreach (var tokenData in sortedTokens)
        {
            TokenReference tokenReference = Instantiate(tokenReferencePrefab, transform);
            Sprite tokenSprite = GetTokenSpriteById(tokenData.tokenId);
            tokenReference.Setup(tokenSprite, tokenData.isFlipped);
        }
    }

    private void ClearTokenReferences()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    public void RefreshTokenReferences()
    {
        ClearTokenReferences();
        SetupTokenReferences();
    }

    [ContextMenu("Randomize & Refresh")]
    public void RandomizeAndRefresh()
    {
        RandomizeTokenCombination();
        // RefreshTokenReferences() is already called inside Randomize
    }

    public Sprite GetTokenSpriteById(int tokenId)
    {
        foreach (var data in tokenManager.TokenMaterialDatas)
        {
            if (data.tokenId == tokenId)
            {
                return data.tokenSprite;
            }
        }
        Debug.LogWarning($"Token sprite not found for Token ID: {tokenId}");
        return null;
    }

    public bool ValidateTokenConsistency()
    {
        var worldCombination = tokenManager.GetDefaultTokenCombination();
        var refList = tokenCombination.tokensInCombination;
        var worldList = worldCombination.tokensInCombination;

        var worldByPos = new Dictionary<Vector2Int, TokenData>();
        foreach (var td in worldList)
        {
            worldByPos[td.gridPosition] = td;
        }

        bool isValid = true;

        // Compare UI refs against world by position
        foreach (var refTd in refList)
        {
            if (!worldByPos.TryGetValue(refTd.gridPosition, out var worldTd))
            {
                Debug.LogError($"Validation: position {refTd.gridPosition} missing in world state.");
                isValid = false;
                continue;
            }

            if (refTd.tokenId != worldTd.tokenId)
            {
                Debug.LogError($"Validation: position {refTd.gridPosition} tokenId mismatch. UI {refTd.tokenId} vs World {worldTd.tokenId}.");
                isValid = false;
            }

            if (refTd.isFlipped != worldTd.isFlipped)
            {
                Debug.LogError($"Validation: position {refTd.gridPosition} flip mismatch. UI {refTd.isFlipped} vs World {worldTd.isFlipped}.");
                isValid = false;
            }
        }

        // Check world positions present but not in UI
        var refByPos = new HashSet<Vector2Int>();
        foreach (var td in refList) refByPos.Add(td.gridPosition);
        foreach (var worldTd in worldList)
        {
            if (!refByPos.Contains(worldTd.gridPosition))
            {
                Debug.LogError($"Validation: position {worldTd.gridPosition} present in world but missing in UI.");
                isValid = false;
            }
        }

        if (isValid)
        {
            Debug.Log("Validation: token combinations match (id, position, flip).");
        }

        return isValid;
    }

    [ContextMenu("Validate Token Consistency")]
    private void ValidateFromContextMenu()
    {
        ValidateTokenConsistency();
    }
}
