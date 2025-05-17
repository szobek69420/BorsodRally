using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RacerClientPositionUpdate : MonoBehaviour
{
    private IngameCarComponents carComponents;
    private NetworkManager networkManager;

    private void Start()
    {
        carComponents=this.gameObject.GetComponent<IngameCarComponents>();
        networkManager=GameObject.Find("NetworkManager")?.GetComponent<NetworkManager>();
    }

    private void FixedUpdate()
    {
        if (carComponents == null||networkManager==null)
            return;

        carComponents.InterpolateOrientation(networkManager.ServerTime.TimeAsFloat, 0.05f);
    }
}
