using DG.Tweening;
using UnityEngine;

public class Token : MonoBehaviour
{
    [SerializeField] private int tokenId;
    [SerializeField] private bool isFlipped = false;
    [SerializeField] private MeshRenderer meshRenderer;

    private Vector2Int gridPosition;
    private TokenManager tokenManager;
    private bool isSwapping;
    private Collider[] colliders;
    private bool isSelected;

    public int TokenId => tokenId;
    public Vector2Int GridPosition => gridPosition;
    public bool IsSwapping => isSwapping;
    public TokenManager TokenManager => tokenManager;
    public MeshRenderer MeshRenderer => meshRenderer;
    public bool IsFlipped => isFlipped;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            meshRenderer.gameObject.layer = isSelected ? LayerMask.NameToLayer("Outline") : LayerMask.NameToLayer("Default");
        }
    }

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
    }

    public void EndSwap()
    {
        isSwapping = false;
    }


    public void Flip()
    {
        isFlipped = !isFlipped;
        Debug.Log($"Token {tokenId} flipped. New state: {isFlipped}");
        transform.DOLocalRotate(new Vector3(0, 0, isFlipped ? 180 : 0), 0.5f);
    }

}
