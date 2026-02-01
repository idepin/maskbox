using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokenReferenceManager : MonoBehaviour
{
    [SerializeField] private TokenManager tokenManager;

    [SerializeField] private TokenCombination tokenCombination;
    [SerializeField] private TokenReference tokenReferencePrefab;
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
        SetupTokenReferences();
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
            if (token == null || token.TokenId != tokenData.tokenId)
            {
                isValid = false;
                break;
            }
        }

        if (isValid)
        {
            Debug.Log("Token combination is valid!");
            currentPoint += 1;
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
        //Randomize grid positions and flip states
        System.Random rand = new System.Random();
        foreach (var tokenData in tokenCombination.tokensInCombination)
        {
            tokenData.gridPosition = new Vector2Int(
                rand.Next(0, tokenManager.GridColumn),
                rand.Next(0, tokenManager.GridRow)
            );
            tokenData.isFlipped = rand.Next(0, 2) == 0;
        }

        RefreshTokenReferences();

        //Update TokenReferences

    }

    private void SetupTokenReferences()
    {
        // Match TokenManager.SetupAllToken order: tokenId ascending (x + y * gridColumn)
        List<TokenData> sortedTokens = new List<TokenData>(tokenCombination.tokensInCombination);
        sortedTokens.Sort((a, b) => a.tokenId.CompareTo(b.tokenId));

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

        var worldById = new Dictionary<int, TokenData>();
        foreach (var td in worldList)
        {
            worldById[td.tokenId] = td;
        }

        bool isValid = true;

        foreach (var refTd in refList)
        {
            if (!worldById.TryGetValue(refTd.tokenId, out var worldTd))
            {
                Debug.LogError($"Validation: tokenId {refTd.tokenId} missing in world state.");
                isValid = false;
                continue;
            }

            if (refTd.gridPosition != worldTd.gridPosition)
            {
                Debug.LogError($"Validation: tokenId {refTd.tokenId} grid mismatch. UI {refTd.gridPosition} vs World {worldTd.gridPosition}.");
                isValid = false;
            }

            if (refTd.isFlipped != worldTd.isFlipped)
            {
                Debug.LogError($"Validation: tokenId {refTd.tokenId} flip mismatch. UI {refTd.isFlipped} vs World {worldTd.isFlipped}.");
                isValid = false;
            }
        }

        var refById = new HashSet<int>();
        foreach (var td in refList) refById.Add(td.tokenId);
        foreach (var worldTd in worldList)
        {
            if (!refById.Contains(worldTd.tokenId))
            {
                Debug.LogError($"Validation: tokenId {worldTd.tokenId} present in world but missing in UI.");
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
