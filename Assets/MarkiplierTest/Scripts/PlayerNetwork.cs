using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log("nigga");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
