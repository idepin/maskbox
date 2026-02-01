using Unity.Netcode;
using UnityEngine;

public class TokenNetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<int> _point = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Rpc(SendTo.Server)]
    public void MarkFinishRpc()
    {
        _point.Value++;
    }
#if UNITY_EDITOR
    [ContextMenu("[DEBUG] Test Add Point")]
    private void DebugTestAddPoint()
    {
        if (!Application.isPlaying) return;
        MarkFinishRpc();
    }
#endif
}
