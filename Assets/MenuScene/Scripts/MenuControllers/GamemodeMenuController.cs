using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamemodeMenuController : MenuController
{
    private enum ActiveCanvas
    {
        GAMEMODE, SINGLEPLAYER, MULTIPLAYER
    };


    //canvases
    [SerializeField] private Canvas canvas_gameMode;
    [SerializeField] private Canvas canvas_singlePlayer;
    [SerializeField] private Canvas canvas_multiPlayerParent;
    [SerializeField] private Canvas canvas_multiPlayerHost;
    [SerializeField] private Canvas canvas_multiPlayerJoin;

    //gamemode things
    [SerializeField] private Button button_singleplayer;
    [SerializeField] private Button button_multiplayer;
    [SerializeField] private Button button_goBack;

    //singleplayer things
    [SerializeField] private Button button_goBackFromSingle;

    [SerializeField] private Button button_launchSingle;

    [SerializeField] private Slider slider_length;
    [SerializeField] private Slider slider_curviness;
    [SerializeField] private Slider slider_difficulty;
    [SerializeField] private TMP_InputField inputField_seed;

    //multiplayer things
    [SerializeField] private Button button_goBackFromMulti;

    [SerializeField] private Button button_multiplayerHost;
    [SerializeField] private Button button_launchMultiHost;

    [SerializeField] private Slider slider_lengthHost;
    [SerializeField] private Slider slider_curvinessHost;
    [SerializeField] private Slider slider_difficultyHost;
    [SerializeField] private TMP_InputField inputField_seedHost;

    [SerializeField] private Button button_multiplayerJoin;

    private ConcurrentList<LobbyScanInfo> availableLobbies = new ConcurrentList<LobbyScanInfo>();
    private Thread lobbySearcherThread = null;

    [SerializeField] private Transform lobbyInfoParent;
    [SerializeField] private GameObject lobbyInfoPrefabs;
    private List<GameObject> instantiatedLobbyInfos = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        button_goBack.onClick.AddListener(() => { GoBackButtonFunction(); PlayClickSound(); });
        button_goBackFromSingle.onClick.AddListener(() => { GoBackFromSingleButtonFunction(); PlayClickSound(); });
        button_goBackFromMulti.onClick.AddListener(() => { GoBackFromMultiButtonFunction(); PlayClickSound(); });

        button_singleplayer.onClick.AddListener(() => { SingleplayerButtonFunction(); PlayClickSound(); });
        button_multiplayer.onClick.AddListener(() => { MultiplayerButtonFunction();  PlayClickSound(); });

        button_multiplayerHost.onClick.AddListener(() => { HostButtonFunction(); PlayClickSound(); });
        button_multiplayerJoin.onClick.AddListener(() => { JoinButtonFunction(); PlayClickSound(); });

        button_launchSingle.onClick.AddListener(() => { PlayClickSound(); StartSingleplayerButtonFunction(); });
        button_launchMultiHost.onClick.AddListener(() => { PlayClickSound(); StartMultiplayerHostButtonFunction(); });

        StartCoroutine(LobbyInfoUpdater());
    }


    public override void Show()
    {
        if (!PlayerPrefs.HasKey("length"))
            PlayerPrefs.SetInt("length", 30);
        if (!PlayerPrefs.HasKey("curviness"))
            PlayerPrefs.SetFloat("curviness", 3f);
        if (!PlayerPrefs.HasKey("difficulty"))
            PlayerPrefs.SetInt("difficulty", 1);
        if (!PlayerPrefs.HasKey("seed"))
            PlayerPrefs.SetInt("seed", 69420);

        int length = PlayerPrefs.GetInt("length");
        float curviness = PlayerPrefs.GetFloat("curviness");
        int diffficulty = PlayerPrefs.GetInt("difficulty");
        
        slider_length.value = length;
        slider_curviness.value = curviness;
        slider_difficulty.value = diffficulty;

        slider_lengthHost.value = length;
        slider_curvinessHost.value = curviness;
        slider_difficultyHost.value = diffficulty;

        inputField_seed.ActivateInputField();
        inputField_seedHost.ActivateInputField();

        SwitchCanvas(ActiveCanvas.GAMEMODE);
    }

    public override void Hide()
    {
        KillLobbySearcherThread();
        base.Hide();
    }

    private void SwitchCanvas(ActiveCanvas choice)
    {
        Hide();

        switch(choice)
        {
            case ActiveCanvas.GAMEMODE:
                canvas_gameMode.enabled = true;
                GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Gamemode();
                break;

            case ActiveCanvas.SINGLEPLAYER:
                canvas_singlePlayer.enabled = true;
                GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Singleplayer();
                break;

            case ActiveCanvas.MULTIPLAYER:
                canvas_multiPlayerParent.enabled = true;
                GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Multiplayer();
                break;
        }
    }

    public void GoBackButtonFunction()
    {
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().MainMenu();

        MenuController mc = GameObject.Find("MainMenuController").GetComponent<MenuController>();

        this.Hide();
        mc.Show();
    }

    public void SingleplayerButtonFunction()
    {
        SwitchCanvas(ActiveCanvas.SINGLEPLAYER);
    }

    public void MultiplayerButtonFunction()
    {
        SwitchCanvas(ActiveCanvas.MULTIPLAYER);
    }

    public void GoBackFromSingleButtonFunction()
    {
        SwitchCanvas(ActiveCanvas.GAMEMODE);
    }

    public void GoBackFromMultiButtonFunction()
    {
        SwitchCanvas(ActiveCanvas.GAMEMODE);
    }

    public void StartSingleplayerButtonFunction()
    {
        PlayerPrefs.SetInt("length", (int)slider_length.value);

        int seed;
        if (int.TryParse(inputField_seed.text, out seed)){
            PlayerPrefs.SetInt("seed", seed);   
        }

        PlayerPrefs.SetFloat("curviness", slider_curviness.value);
        PlayerPrefs.SetInt("difficulty", (int)slider_difficulty.value);
        
        SceneManager.LoadSceneAsync("TrackGen");
    }

    public void StartMultiplayerHostButtonFunction()
    {
        KillLobbySearcherThread();

        PlayerPrefs.SetInt("length", (int)slider_lengthHost.value);

        int seed;
        if (int.TryParse(inputField_seedHost.text, out seed))
        {
            PlayerPrefs.SetInt("seed", seed);
        }

        PlayerPrefs.SetFloat("curviness", slider_curvinessHost.value);
        PlayerPrefs.SetInt("difficulty", (int)slider_difficultyHost.value);

        SceneManager.LoadSceneAsync("Multiplayer");
    }

    public void StartMultiplayerJoinButtonFunction(AvailableLobby lobbyInfo)
    {

    }

    public void HostButtonFunction()
    {
        canvas_multiPlayerJoin.enabled = false;
        canvas_multiPlayerHost.enabled = true;

        KillLobbySearcherThread();
    }

    public void JoinButtonFunction()
    {
        canvas_multiPlayerHost.enabled = false;
        canvas_multiPlayerJoin.enabled = true;

        StartLobbySearcherThread();
    }

    private void SearchForAvailableLobbies()
    {
        using (UdpClient client = new UdpClient())//so that the socket is yeeted automatically
        {
            int port = 42069;
            IPEndPoint localEP = null;
            while(true)//try as long as we find a free port
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
            client.Client.ReceiveTimeout = 100;


            long lastMessageTime = 0;
            int scanCount = 0;
            Stopwatch timer= new Stopwatch();
            timer.Start();

            while (true)
            {
                timer.Stop();
                lastMessageTime += timer.ElapsedMilliseconds;
                timer.Restart();

                if(lastMessageTime>1000)
                {
                    //update variables
                    lastMessageTime = 0;
                    scanCount++;

                    //yeet the lobbies whose host didn't respond for a long time
                    for(int i=0;i<availableLobbies.Count;i++)
                    {
                        if (scanCount - availableLobbies[i].lastScanCount > 3)
                            availableLobbies.RemoveAt(i--);
                    }

                    //scan the network
                    byte[] joinMsg = Encoding.ASCII.GetBytes("yo i wanna join "+scanCount.ToString());
                    for (int i=42666;i<42671;i++)//only scans the first 5 possible addresses
                        client.Send(joinMsg, joinMsg.Length, new IPEndPoint(IPAddress.Broadcast, i));
                }

                IPEndPoint remoteEP=null;

                try
                {
                    byte[] reply=client.Receive(ref remoteEP);

                    string replyMsg = Encoding.ASCII.GetString(reply);
                    LobbyScanInfo replyData=LobbyScanInfo.ParseString(replyMsg);

                    //check if the reply is about an already registered lobby
                    bool lobbyAlreadyExists = false;
                    for(int i=0;i<availableLobbies.Count;i++)
                    {
                        if (availableLobbies[i].lobbyInfo.ip.Equals(replyData.lobbyInfo.ip))
                        {
                            availableLobbies[i] = replyData;
                            lobbyAlreadyExists = true;
                            break;
                        }
                    }

                    //if not, register it
                    if(!lobbyAlreadyExists)
                        availableLobbies.Add(replyData);
                }
                catch(SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut)
                        continue;

                    UnityEngine.Debug.Log(se.ToString());
                }
            }
        }
    }

    private void KillLobbySearcherThread()
    {
        if (lobbySearcherThread?.IsAlive == true)
            lobbySearcherThread.Abort();

        if (lobbySearcherThread != null)
            lobbySearcherThread = null;

        //Debug.Log("lobby searcher killed");
    }

    private void StartLobbySearcherThread()
    {
        //are lobbies already searched?
        if (lobbySearcherThread != null && lobbySearcherThread.IsAlive)
            return;

        availableLobbies.Clear();

        //start a lobby searcher thread
        lobbySearcherThread = new Thread(SearchForAvailableLobbies);
        lobbySearcherThread.IsBackground = true;
        lobbySearcherThread.Start();

        //Debug.Log("lobby searcher started");
    }

    private IEnumerator LobbyInfoUpdater()
    {
        try
        {
            if(canvas_multiPlayerJoin.enabled)
            {
                foreach(GameObject lobbyInfoInstance in instantiatedLobbyInfos)
                    Destroy(lobbyInfoInstance);
                instantiatedLobbyInfos.Clear();

                float currentPositionY = availableLobbies.Count / 2.0f;

                for(int i=0;i<availableLobbies.Count;i++, currentPositionY-=1.0f)
                {
                    GameObject instance = Instantiate(lobbyInfoPrefabs, lobbyInfoParent);
                    instance.transform.localPosition = new Vector3(0, currentPositionY, 0);
                    instance.transform.localRotation = Quaternion.identity;
                    instance.GetComponent<LobbyInfoUI>().Initialize(availableLobbies[i].lobbyInfo, StartMultiplayerJoinButtonFunction);
                    instantiatedLobbyInfos.Add(instance);
                }
            }
        }
        catch (Exception ex) { }

        yield return new WaitForSeconds(1.0f);
        StartCoroutine(LobbyInfoUpdater());
    }
}
