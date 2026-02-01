using Unity.Netcode;
using UnityEngine;

public class TokenNetworkPlayer : NetworkBehaviour
{
    [Header("Requirements")]
    [SerializeField]
    private UI_PlayerScoreLeaderboardDisplay _playerScoreDisplay = null;

    [Header("Properties")]
    [SerializeField]
    private NetworkVariable<int> _point = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Runtime Debug")]
    [SerializeField]
    private UI_LeaderboardView _leaderboardView = null;

    // Runtime variable data.
    private UI_PlayerScoreLeaderboardDisplay _scoreDisplayInstance = null;
    private TokenReferenceManager _tokenRefManager = null;

    private void Awake()
    {
        // Subscribe events.
        _point.OnValueChanged += OnScoreChanged;
    }

    public override void OnDestroy()
    {
        // Subscribe events.
        _point.OnValueChanged -= OnScoreChanged;
        if (_tokenRefManager != null)
            _tokenRefManager.OnPointChanged -= ListenOnPointValueChanged;

        base.OnDestroy();
    }

    private void OnScoreChanged(int previousValue, int newValue)
    {
        _scoreDisplayInstance.SetScore(newValue);
    }

    public override void OnNetworkSpawn()
    {
        _leaderboardView = FindAnyObjectByType<UI_LeaderboardView>();
        _scoreDisplayInstance = Instantiate(_playerScoreDisplay, _leaderboardView.PlayerScoreContainer);

        _scoreDisplayInstance.SetName($"Player {OwnerClientId}");
        _scoreDisplayInstance.SetScore(_point.Value);

        if (IsOwner)
        {
            _tokenRefManager = FindAnyObjectByType<TokenReferenceManager>();
            _tokenRefManager.OnPointChanged += ListenOnPointValueChanged;
        }
    }

    private void ListenOnPointValueChanged(int pointNow) => OnPointRpc(pointNow);

    [Rpc(SendTo.Server)]
    public void OnPointRpc(int currentPoint)
    {
        _point.Value = currentPoint;
    }
#if UNITY_EDITOR
    [ContextMenu("[DEBUG] Test Add Point")]
    private void DebugTestAddPoint()
    {
        if (!Application.isPlaying) return;
        OnPointRpc(_point.Value + 1);
    }
#endif
}
