using DG.Tweening;
using UnityEngine;

public class Token : MonoBehaviour
{
    [SerializeField] private int tokenId;
    [SerializeField] private bool isFlipped = false;

    private Vector2Int gridPosition;
    private TokenManager tokenManager;
    private bool isSwapping;
    private Collider[] colliders;

    public int TokenId => tokenId;
    public bool IsFlipped => isFlipped;
    public Vector2Int GridPosition => gridPosition;
    public bool IsSwapping => isSwapping;

    void Start()
    {
        tokenManager = GetComponentInParent<TokenManager>();
        colliders = GetComponentsInChildren<Collider>();
    }

    public void Setup(int tokenId, Vector2Int gridPosition)
    {
        this.tokenId = tokenId;
        this.gridPosition = gridPosition;
        gameObject.name = "Token_" + tokenId;
    }

    public void BeginSwap()
    {
        isSwapping = true;
        SetCollidersEnabled(false);
    }

    public void EndSwap()
    {
        SetCollidersEnabled(true);
        isSwapping = false;
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (colliders == null || colliders.Length == 0)
        {
            colliders = GetComponentsInChildren<Collider>();
        }
        foreach (var c in colliders)
        {
            c.enabled = enabled;
        }
    }

    public void Flip()
    {
        isFlipped = !isFlipped;
        Debug.Log($"Token {tokenId} flipped. New state: {isFlipped}");
        transform.DOLocalRotate(new Vector3(0, 0, isFlipped ? 180 : 0), 0.5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isSwapping) return;
        Token otherToken = other.GetComponentInParent<Token>();
        if (otherToken == null) return;
        if (otherToken == this) return;
        if (otherToken.IsSwapping) return;
        if (tokenManager == null) return;
        Debug.Log($"Token {tokenId} collided with Token {otherToken.TokenId}");
        tokenManager.SwapTokens(this, otherToken, null);
    }
}
