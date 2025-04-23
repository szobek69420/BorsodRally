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
    public ushort localPort = 0;

    private void Start()
    {
        int processId = Process.GetCurrentProcess().Id;

        if (PlayerPrefs.HasKey("isHost"+processId) && PlayerPrefs.GetInt("isHost"+processId) != 0)
        {
            //get local address and port
            localAddress =GetLocalAddress();
            localPort = (ushort)Random.Range(50000, 60000);

            //set the unity transport address to the local address
            UnityTransport ut=this.gameObject.GetComponent<UnityTransport>();
            ut.SetConnectionData(localAddress.ToString(), localPort);

            //zsa
            this.gameObject.GetComponent<NetworkManager>().StartHost();
            this.gameObject.GetComponent<GameManagerMultiplayerSpawner>().OnServerStarted();
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

    //connects to a DNS server to find the interface the machine is using
    private IPAddress GetLocalAddress()
    {
        IPAddress localAddress = null;

        try
        {
            UdpClient addressObtainer = new UdpClient(42042);
            addressObtainer.Connect(new IPEndPoint(IPAddress.Parse("4.2.2.2"), 60000));
            IPEndPoint localEndpoint = addressObtainer.Client.LocalEndPoint as IPEndPoint;
            localAddress = localEndpoint.Address;
            addressObtainer.Close();
        }
        catch (SocketException se)
        {
            localAddress = IPAddress.Any;
        }

        return localAddress;
    }
}
