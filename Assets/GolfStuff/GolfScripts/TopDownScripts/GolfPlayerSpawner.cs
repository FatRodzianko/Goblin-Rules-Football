using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class GolfPlayerSpawner : MonoBehaviour
{
    private NetworkManager _networkManager;
    [SerializeField] GameObject _playerPrefab;
    private void Start()
    {
        InitializeOnce();
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }


    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        if (_playerPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }


        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, true);
        nob.transform.SetPositionAndRotation(new Vector3(UnityEngine.Random.Range(0f,1f), UnityEngine.Random.Range(-1f, 1f),0f), Quaternion.identity);
        _networkManager.ServerManager.Spawn(nob, conn);

    }
}
