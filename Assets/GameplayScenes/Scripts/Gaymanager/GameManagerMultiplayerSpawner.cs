using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManagerMultiplayerSpawner : MonoBehaviour
{
    public GameObject prefab;

    void Awake()
    {
        GameObject.Find("NetworkManager").GetComponent<NetworkManager>().OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        if (GameObject.Find("NetworkManager").GetComponent<NetworkManager>().IsHost)
        {
            // Check if an instance already exists in the scene
            if (GameManagerMultiplayer.Singleton == null)
            {
                GameObject spawnedObject = Instantiate(prefab);
                NetworkObject networkObject = spawnedObject.GetComponent<NetworkObject>();
                networkObject.Spawn();
                Debug.Log("Singleton Network Object spawned on the server.");
            }
            else
            {
                Debug.Log("Singleton Network Object already exists on the server.");
            }
        }
    }
}
