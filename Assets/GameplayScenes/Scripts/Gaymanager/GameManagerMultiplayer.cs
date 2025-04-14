using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerMultiplayer : GameManagerBase
{
    [SerializeField] private GameObject carPrefab_player;
    [SerializeField] private GameObject carPrefab_opponent;

    //lobby things

    //countdown things
    [SerializeField] private Canvas canvas_countdown;
    [SerializeField] private TMP_Text text_countdown;

    //ingame things
    [SerializeField] private Canvas canvas_ingame;
    [SerializeField] private SpeedometerHandler speedo;
    [SerializeField] private TMP_Text text_progress;

    private float greatestProgress = 0;

    //end things
    [SerializeField] private Canvas canvas_end;
    [SerializeField] private TMP_Text text_position;
    [SerializeField] private Button button_returnToMenu;

    public override void OnNetworkSpawn()
    {
        InitScene();
    }

    public override void OnDestroy()
    {
        KillLobbyResponderThread();//necessary for debugging, because the editor can't automatically close threads on stop
    }

    protected override void InitScene()
    {
        //get the track generator
        GetTrackManager();

        //start lobby responder if necessary
        if(IsHost)
        {
            MultiplayerStarter ms=GameObject.Find("NetworkManager")?.GetComponent<MultiplayerStarter>();
            hostAddress = ms.localAddress;
            hostPort= ms.localPort;

            StartLobbyResponderThread();
        }
        
        //generate track
        track.FetchParameters();
        track.StartGen();

        //start countdown
        ShowLobbyScreen();
    }

    protected override void ShowLobbyScreen()
    {
        State = GameState.LOBBY;
    }

    protected override void UpdateLobbyScreen()
    {
        //the singleplayer mode doesn't need a lobby screen
    }

    protected override void StartCountdown()
    {
        State = GameManagerBase.GameState.COUNTDOWN;
    }

    protected override void UpdateCountdownScreen()
    {

    }
    protected override void StartRace()
    {
        State = GameManagerBase.GameState.RACE;
    }
    protected override void UpdateRaceScreen()
    {
        
    }

    public override void EndRace()
    {
        State = GameManagerBase.GameState.END;

    }

    protected override void UpdateEndScreen()
    {

    }

    protected override void ReturnToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }



    //lobby responder things----------------------------------------------------------------------------------------------
    //this is the thread that is responsibly for responding to the lobby searcher thread in the menu
    private Thread lobbyResponderThread = null;
    private IPAddress hostAddress = null;
    private ushort hostPort = 0;

    public void StartLobbyResponderThread()
    {
        if (lobbyResponderThread != null && lobbyResponderThread.IsAlive)
            return;

        lobbyResponderThread = new Thread(LobbyResponderThread);
        lobbyResponderThread.IsBackground = true;
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
            int port = 42666;
            IPEndPoint localEP = null;
            while (true)//try as long as we find a free port
            {
                try
                {
                    localEP = new IPEndPoint(IPAddress.Any, port);
                    client.Client.Bind(localEP);
                    break;
                }
                catch (SocketException se)
                {
                    port++;
                }
            }

            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    byte[] request = client.Receive(ref remoteEP);
                    string requestString = Encoding.ASCII.GetString(request);

                    if (requestString.Substring(0, 16).Equals("yo i wanna join "))//it is a request from a searcher thread
                    {
                        int requestScanCount = System.Convert.ToInt32(requestString.Substring(16));

                        LobbyScanInfo replyData = new LobbyScanInfo(
                            new AvailableLobby(new IPEndPoint(hostAddress, port), "Water Weight", 1, 4),
                            requestScanCount
                            );
                        byte[] reply = Encoding.ASCII.GetBytes(replyData.ToString());
                        client.Send(reply, reply.Length, remoteEP);
                    }
                    else if(requestString.Equals("i am approaching"))
                    {
                        LobbyTrackInfo lti = track.SerializeParameters();
                        lti.ip = new IPEndPoint(hostAddress, hostPort);

                        byte[] reply=Encoding.ASCII.GetBytes(lti.ToString());
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
