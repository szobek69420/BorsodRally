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
    [SerializeField] private Canvas canvas_multiPlayerDirectJoin;

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

    [SerializeField] private Button button_multiplayerDirectJoin;
    [SerializeField] private TMP_InputField inputField_joinAddress;
    [SerializeField] private Button button_launchDirectJoin;

    private object directJoinSynchronizer=new object();
    private LobbyTrackInfo directJoinTrackInfo = null;

    //multiplayer interface selectors
    [SerializeField] private Button button_chooseInterfaceJoin;
    [SerializeField] private TMP_Text text_interfaceJoin;
    [SerializeField] private Button button_chooseInterfaceHost;
    [SerializeField] private TMP_Text text_interfaceHost;
    private LocalAddressQuerier.NetworkInterfaceInfo[] activeInterfaces = null;
    private int usedInterfaceIndex = 0;

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
        button_multiplayerDirectJoin.onClick.AddListener(() => { DirectJoinButtonFunction(); PlayClickSound(); });

        button_launchSingle.onClick.AddListener(() => { PlayClickSound(); StartSingleplayerButtonFunction(); });
        button_launchMultiHost.onClick.AddListener(() => { PlayClickSound(); StartMultiplayerHostButtonFunction(); });
        button_launchDirectJoin.onClick.AddListener(() => { PlayClickSound(); StartDirectJoinButtonFunction(); });

        button_chooseInterfaceJoin.onClick.AddListener(() => { PlayClickSound(); ChangeInterfaceButtonJoinFunction(); });
        button_chooseInterfaceHost.onClick.AddListener(() => { PlayClickSound(); ChangeInterfaceButtonHostFunction(); });

        inputField_joinAddress.onValueChanged.AddListener((string sus) => { JoinAddressInputFieldOnValueChangedFunction(); });

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
        int length = (int)slider_length.value;
        int seed = 0;
        int.TryParse(inputField_seed.text, out seed);
        float curviness = slider_curviness.value;
        int difficulty = (int)slider_difficulty.value;

        //set the menu values
        PlayerPrefs.SetInt("length", length);
        PlayerPrefs.SetInt("seed", seed);
        PlayerPrefs.SetFloat("curviness", curviness);
        PlayerPrefs.SetInt("difficulty", difficulty);

        //set the values for the game scene (they have to be process specific)
        int processId=Process.GetCurrentProcess().Id;
        PlayerPrefs.SetInt("length"+processId, length);
        PlayerPrefs.SetInt("seed" + processId, seed);
        PlayerPrefs.SetFloat("curviness" + processId, curviness);
        PlayerPrefs.SetInt("difficulty" + processId, difficulty);

        //load scene
        SceneManager.LoadSceneAsync("Singleplayer");
    }

    public void StartMultiplayerHostButtonFunction()
    {
        KillLobbySearcherThread();

        int length = (int)slider_length.value;
        int seed = 0;
        int.TryParse(inputField_seed.text, out seed);
        float curviness = slider_curviness.value;
        int difficulty = (int)slider_difficulty.value;

        //set the menu values
        PlayerPrefs.SetInt("length", length);
        PlayerPrefs.SetInt("seed", seed);
        PlayerPrefs.SetFloat("curviness", curviness);
        PlayerPrefs.SetInt("difficulty", difficulty);

        //set the values for the game scene (they have to be process specific)
        int processId = Process.GetCurrentProcess().Id;
        PlayerPrefs.SetInt("length" + processId, length);
        PlayerPrefs.SetInt("seed" + processId, seed);
        PlayerPrefs.SetFloat("curviness" + processId, curviness);
        PlayerPrefs.SetInt("difficulty" + processId, difficulty);
        PlayerPrefs.SetInt("isHost" + processId, 69);
        if(activeInterfaces != null&&activeInterfaces.Length > 0)
            PlayerPrefs.SetString("hostAddress" + processId, activeInterfaces[usedInterfaceIndex].Address.ToString());
        else
            PlayerPrefs.SetString("hostAddress" + processId, IPAddress.Any.ToString());

        PlayerPrefs.SetString("name" + processId, PlayerPrefs.GetString("name"));

        //load scene
        SceneManager.LoadSceneAsync("Multiplayer");
    }

    public void StartMultiplayerJoinButtonFunction(AvailableLobby lobbyInfo)
    {
        //query the track parameters from the host
        LobbyTrackInfo lti = null;
        try
        {
            byte[] message= Encoding.ASCII.GetBytes("i am approaching");

            UdpClient client = new UdpClient();
            client.Client.Bind(new IPEndPoint(activeInterfaces[usedInterfaceIndex].Address, UnityEngine.Random.Range(55000, 60000)));
            client.Client.ReceiveTimeout = 500;
            client.Send(message, message.Length, lobbyInfo.ip);

            IPEndPoint remoteEp = null;
            byte[] response=client.Receive(ref remoteEp);
            lti = LobbyTrackInfo.ParseString(Encoding.ASCII.GetString(response));
            
        }
        catch(Exception ex)
        {
            return;
        }

        KillLobbySearcherThread();

        //set the track parameters for the game scene
        int processId = Process.GetCurrentProcess().Id;
        PlayerPrefs.SetInt("length" + processId, lti.length);
        PlayerPrefs.SetInt("seed" + processId, lti.seed);
        PlayerPrefs.SetFloat("curviness" + processId, lti.curviness);
        PlayerPrefs.SetInt("difficulty" + processId, lti.difficulty);
        PlayerPrefs.SetInt("isHost" + processId, 0);
        PlayerPrefs.SetString("hostAddress"+processId, lti.ip.Address.ToString());
        PlayerPrefs.SetInt("hostPort"+processId, lti.ip.Port);

        PlayerPrefs.SetString("name" + processId, PlayerPrefs.GetString("name"));

        //load scene
        SceneManager.LoadScene("Multiplayer");
    }

    public void HostButtonFunction()
    {
        KillLobbySearcherThread();

        if (!LocalAddressQuerier.GetLocalAddresses(out activeInterfaces))
            UnityEngine.Debug.Log("No available network interface");
        else
        {
            usedInterfaceIndex = 0;

            ChangeInterfaceButtonHostFunction();
        }

        canvas_multiPlayerJoin.enabled = false;
        canvas_multiPlayerDirectJoin.enabled = false;
        canvas_multiPlayerHost.enabled = true;
    }

    public void JoinButtonFunction()
    {
        canvas_multiPlayerHost.enabled = false;
        canvas_multiPlayerDirectJoin.enabled = false;
        canvas_multiPlayerJoin.enabled = true;

        StartLobbySearcherThread();
    }

    public void DirectJoinButtonFunction()
    {
        //kill lobby searcher thread just in case
        KillLobbySearcherThread();

        //query active interfaces
        LocalAddressQuerier.GetLocalAddresses(out activeInterfaces);

        //trigger recolouring
        inputField_joinAddress.text = "";
        inputField_joinAddress.text = "0.0.0.0";

        canvas_multiPlayerHost.enabled = false;
        canvas_multiPlayerJoin.enabled = false;
        canvas_multiPlayerDirectJoin.enabled = true;
    }


    public void ChangeInterfaceButtonHostFunction()
    {
        if (activeInterfaces == null || activeInterfaces.Length == 0)
            return;

        usedInterfaceIndex++;
        if(usedInterfaceIndex>=activeInterfaces.Length)
            usedInterfaceIndex%=activeInterfaces.Length;

        text_interfaceHost.text = activeInterfaces[usedInterfaceIndex].Name;
    }

    public void StartDirectJoinButtonFunction()
    {
        //check if the given address is valid
        IPAddress address;
        LocalAddressQuerier.NetworkInterfaceInfo chosenInterface = null;

        if (!IPAddress.TryParse(inputField_joinAddress.text, out address))
            return;

        if (activeInterfaces == null || activeInterfaces.Length == 0)
            return;
        foreach (var interspar in activeInterfaces)
        {
            if (!interspar.IsAddressOnSameNetwork(address))
                continue;

            chosenInterface = interspar;
            break;
        }
        if (chosenInterface == null)
            return;

        //create the join query threads (one thread per possible host port)
        directJoinTrackInfo = null;
        List<Thread> threads = new List<Thread>();

        for (int i = 42666; i < 42671; i++)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(this.StartDirectJoinButtonFunctionThreadFunc));
            thread.Start((new IPEndPoint[2] {new IPEndPoint(chosenInterface.Address, 0),new IPEndPoint(address, i)}) as object);
            threads.Add(thread);
        }

        foreach(var thread in threads)
            thread.Join();

        //check if there was a great success
        if (directJoinTrackInfo == null)
        {
            return;
        }

        //set the track parameters for the game scene
        int processId = Process.GetCurrentProcess().Id;
        PlayerPrefs.SetInt("length" + processId, directJoinTrackInfo.length);
        PlayerPrefs.SetInt("seed" + processId, directJoinTrackInfo.seed);
        PlayerPrefs.SetFloat("curviness" + processId, directJoinTrackInfo.curviness);
        PlayerPrefs.SetInt("difficulty" + processId, directJoinTrackInfo.difficulty);
        PlayerPrefs.SetInt("isHost" + processId, 0);
        PlayerPrefs.SetString("hostAddress" + processId, directJoinTrackInfo.ip.Address.ToString());
        PlayerPrefs.SetInt("hostPort" + processId, directJoinTrackInfo.ip.Port);

        PlayerPrefs.SetString("name" + processId, PlayerPrefs.GetString("name"));

        //load scene
        SceneManager.LoadScene("Multiplayer");
    }

    private void StartDirectJoinButtonFunctionThreadFunc(object param)
    {
        IPEndPoint[] addresses=param as IPEndPoint[];
        IPAddress clientAddress = addresses[0].Address;
        IPEndPoint hostAddress = addresses[1];

        LobbyTrackInfo lti = null;
        
        using (UdpClient udpClient = new UdpClient())
        {
            //try to bind to a port
            int triesLeft = 10;
            System.Random random = new System.Random();
            while (true)
            {
                triesLeft--;
                try { udpClient.Client.Bind(new IPEndPoint(clientAddress, random.Next(50000, 60000))); }
                catch (Exception e) { if (triesLeft <= 0) return; continue; }
                break;
            }

            //send and receive message
            try
            {
                byte[] messageBytes = Encoding.ASCII.GetBytes("i am approaching");
                udpClient.Client.ReceiveTimeout = 500;
                udpClient.Send(messageBytes, messageBytes.Length, hostAddress);

                IPEndPoint remoteEp = null;
                byte[] response = udpClient.Receive(ref remoteEp);
                lti = LobbyTrackInfo.ParseString(Encoding.ASCII.GetString(response));
            }
            catch (Exception e)
            {
                return;
            }

            //packet received
            if (lti == null)
                return;

            lock(directJoinSynchronizer)
            {
                directJoinTrackInfo = lti;
            }
        }
    }

    public void ChangeInterfaceButtonJoinFunction()
    {
        usedInterfaceIndex++; //no need to clamp, the startlobbysearcherthread does it
        KillLobbySearcherThread();
        StartLobbySearcherThread();
    }

    private void JoinAddressInputFieldOnValueChangedFunction()
    {
        bool validAddress = false;
        IPAddress address;
        
        //check if it is a valid address
        if (!IPAddress.TryParse(inputField_joinAddress.text, out address))
            goto SetJoinAddressInputFieldColour;
        
        //check if the address is on the same network as us
        if(activeInterfaces==null || activeInterfaces.Length == 0)
            goto SetJoinAddressInputFieldColour;

        foreach(var interspar in  activeInterfaces)
            validAddress = validAddress||interspar.IsAddressOnSameNetwork(address);

        SetJoinAddressInputFieldColour:
        if (validAddress)
            inputField_joinAddress.textComponent.color = Color.white;
        else
            inputField_joinAddress.textComponent.color = Color.red;
    }

    //lobby searcher thread things------------------------------------------
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
                    localEP = new IPEndPoint(activeInterfaces[usedInterfaceIndex].Address, port);
                    //localEP = new IPEndPoint(IPAddress.Parse("172.23.196.171"), port);
                    client.Client.Bind(localEP);
                    break;
                }
                catch (SocketException se)
                {
                    port++;
                }

                try
                {
                    client.Close();
                }
                catch (Exception e) { }
            }
            client.Client.ReceiveTimeout = 100;
            client.Client.EnableBroadcast = true;
            UnityEngine.Debug.Log("sugus " + client.Client.LocalEndPoint.ToString());

            long lastMessageTime = 1000;
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
                    byte[] joinMsg = Encoding.ASCII.GetBytes("yo i wanna join&&"+scanCount.ToString());
                    for (int i=42666;i<42671;i++)//only scans the first 5 possible addresses
                        //client.Send(joinMsg, joinMsg.Length, new IPEndPoint(IPAddress.Parse("172.23.217.247"), i));
                        client.Send(joinMsg, joinMsg.Length, new IPEndPoint(activeInterfaces[usedInterfaceIndex].BroadcastAddress, i));
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

        if(!LocalAddressQuerier.GetLocalAddresses(out activeInterfaces))
        {
            UnityEngine.Debug.LogError("No usable network interfaces bozo");
            text_interfaceJoin.text = "no interface";
            return;
        }

        if (usedInterfaceIndex < 0)
            usedInterfaceIndex = 0;
        else if (usedInterfaceIndex >= activeInterfaces.Length)
            usedInterfaceIndex %= activeInterfaces.Length;
        text_interfaceJoin.text = activeInterfaces[usedInterfaceIndex].Name;

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
