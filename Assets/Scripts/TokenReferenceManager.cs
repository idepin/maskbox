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
}
