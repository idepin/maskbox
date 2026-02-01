using System;
using System.Collections.Generic;
using UnityEngine;

public class TokenReferenceManager : MonoBehaviour
{
    [SerializeField] private TokenManager tokenManager;

    [SerializeField] private TokenCombination tokenCombination;
    [SerializeField] private TokenReference tokenReferencePrefab;
    [SerializeField] private List<TokenImageData> tokenImageDatas;



    void Start()
    {
        tokenCombination = tokenManager.GetDefaultTokenCombination();
        SetupTokenReferences();
    }

    private void SetupTokenReferences()
    {
        foreach (var tokenData in tokenCombination.tokensInCombination)
        {
            TokenReference tokenReference = Instantiate(tokenReferencePrefab, transform);
            Sprite tokenSprite = GetTokenSpriteById(tokenData.tokenId);
            tokenReference.Setup(tokenSprite, tokenData.isFlipped);
        }
    }

    public Sprite GetTokenSpriteById(int tokenId)
    {
        foreach (var data in tokenImageDatas)
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

[Serializable]
public class TokenImageData
{
    public int tokenId;
    public Sprite tokenSprite;
}
