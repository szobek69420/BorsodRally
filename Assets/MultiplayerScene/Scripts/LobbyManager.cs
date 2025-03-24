using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using Unity.VisualScripting;

public class LobbyManager : MonoBehaviour
{
    //this is the thread that is responsibly for responding to the lobby searcher thread in the menu
    private Thread lobbyResponderThread = null;

    private IPAddress localAddress = null;

    // Start is called before the first frame update
    void Start()
    {
        StartLobbyResponderThread();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLobbyResponderThread()
    {
        if (lobbyResponderThread != null && lobbyResponderThread.IsAlive)
            return;

        //get ip address (connect to a dns server)
        try
        {
            UdpClient addressObtainer = new UdpClient(42042);
            addressObtainer.Connect(new IPEndPoint(IPAddress.Parse("4.2.2.2"), 60000));
            IPEndPoint localEndpoint = addressObtainer.Client.LocalEndPoint as IPEndPoint;
            localAddress = localEndpoint.Address;
            addressObtainer.Close();
        }
        catch(SocketException se)
        {
            localAddress = IPAddress.Any;
        }


        lobbyResponderThread= new Thread(LobbyResponderThread);
        lobbyResponderThread.IsBackground=true;
        lobbyResponderThread.Start();
    }

    public void KillLobbyResponderThread()
    {
        if (lobbyResponderThread?.IsAlive == true)
            lobbyResponderThread.Abort();

        if (lobbyResponderThread != null)
            lobbyResponderThread = null;
    }

    public void LobbyResponderThread()
    {
        using (UdpClient client = new UdpClient())//so that the socket is yeeted automatically
        {
            IPEndPoint localEP = new IPEndPoint(localAddress, 42666);
            client.Client.Bind(localEP);
            client.Client.ReceiveTimeout = 100;

            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    byte[] request = client.Receive(ref remoteEP);
                    string requestString = Encoding.ASCII.GetString(request);

                    if(requestString.Equals("yo i wanna join"))//it is a request from a searcher thread
                    {
                        AvailableLobby replyData = new AvailableLobby(
                            new IPEndPoint(localAddress, 42666),
                            "robloxman",
                            1,
                            4
                            );
                        byte[] reply = Encoding.ASCII.GetBytes(replyData.ToString());
                        client.Send(reply, reply.Length, remoteEP);
                    }
                }
                catch (SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut)
                        continue;

                    Debug.Log(se.ToString());
                }
            }
        }
    }
}
