using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class MultiplayerStarter : MonoBehaviour
{
    public IPAddress localAddress = null;
    public IPAddress broadcastAddress = null;
    public ushort localPort = 0;

    private void Start()
    {
        int processId = Process.GetCurrentProcess().Id;

        if (PlayerPrefs.HasKey("isHost"+processId) && PlayerPrefs.GetInt("isHost"+processId) != 0)
        {
            //get local address and port
            if(LocalAddressQuerier.GetLocalAddress(out localAddress, out broadcastAddress))
            {
                localPort = (ushort)Random.Range(50000, 60000);
            }
            else
            {
                localAddress = IPAddress.Any;
                broadcastAddress = IPAddress.Broadcast;
                localPort = 0;
            }

            //set the unity transport address to the local address
            UnityTransport ut = this.gameObject.GetComponent<UnityTransport>();
            ut.SetConnectionData(localAddress.ToString(), localPort);

            //zsa
            this.gameObject.GetComponent<NetworkManager>().StartHost();
        }
        else
        {
            //get local address and port
            localAddress = IPAddress.Parse(PlayerPrefs.GetString("hostAddress"+processId));
            localPort = (ushort)PlayerPrefs.GetInt("hostPort" + processId);

            //set the unity transport address to the local address
            UnityTransport ut = this.gameObject.GetComponent<UnityTransport>();
            ut.SetConnectionData(localAddress.ToString(), localPort);

            //zsa
            this.gameObject.GetComponent<NetworkManager>().StartClient();
        }
    }
}
