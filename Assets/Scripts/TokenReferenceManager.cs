using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokenReferenceManager : MonoBehaviour
{
    [SerializeField] private TokenManager tokenManager;

    [SerializeField] private TokenCombination tokenCombination;
    [SerializeField] private TokenReference tokenReferencePrefab;




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
        //RandomizeTokenCombination();
    }


    //RANDOMIZATION COMBINATION
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
