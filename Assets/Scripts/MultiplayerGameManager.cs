using Unity.Netcode;
using UnityEngine;

public class MultiplayerGameManager : MonoBehaviour
{
    private static MultiplayerGameManager s_instance = null;
    private NetworkManager _networkManager = null;

    public static MultiplayerGameManager Instance => s_instance;

    private void Awake()
    {
        if (s_instance != null)
        {
            Destroy(this);
            return;
        }

        s_instance = this;
        _networkManager = FindAnyObjectByType<NetworkManager>();
    }

    private void OnDestroy()
    {
        if (s_instance != this) return;
        s_instance = null;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!_networkManager.IsClient && !_networkManager.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
            SubmitNewPosition();
        }

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        if (GUILayout.Button("Host")) _networkManager.StartHost();
        if (GUILayout.Button("Client")) _networkManager.StartClient();
        if (GUILayout.Button("Server")) _networkManager.StartServer();
    }

    private void StatusLabels()
    {
        var mode = _networkManager.IsHost ?
            "Host" : _networkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            _networkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    private void SubmitNewPosition()
    {
        if (GUILayout.Button(_networkManager.IsServer ? "Move" : "Request Position Change"))
        {
            if (_networkManager.IsServer && !_networkManager.IsClient)
            {
                // foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                //     m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
            }
            else
            {
                // var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                // var player = playerObject.GetComponent<HelloWorldPlayer>();
                // player.Move();
            }
        }
    }
}
