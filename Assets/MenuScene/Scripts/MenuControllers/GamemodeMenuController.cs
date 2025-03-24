using System;
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

public class GamemodeMenuController : MenuController
{
    [SerializeField] private Button button_goBack;
    [SerializeField] private Button button_goBackFromSingle;

    [SerializeField] private Button button_singleplayer;
    [SerializeField] private Button button_multiplayer;

    [SerializeField] private Button button_start;

    [SerializeField] private Slider slider_length;
    [SerializeField] private Slider slider_curviness;
    [SerializeField] private Slider slider_difficulty;

    [SerializeField] private TMP_InputField inputField_seed;

    [SerializeField] private enum ActiveCanvas { GAMEMODE, SINGLE, MULTI};

    private ConcurrentList<AvailableLobby> availableLobbies=new ConcurrentList<AvailableLobby>();
    private Thread lobbySearcherThread = null;
    

    // Start is called before the first frame update
    void Start()
    {
        button_goBack.onClick.AddListener(() => { GoBackButtonFunction(); PlayClickSound(); });
        button_goBackFromSingle.onClick.AddListener(() => { GoBackFromSingleButtonFunction(); PlayClickSound(); });

        button_singleplayer.onClick.AddListener(() => { SingleplayerButtonFunction(); PlayClickSound(); });
        button_multiplayer.onClick.AddListener(() => { PlayClickSound(); });

        button_start.onClick.AddListener(() => { PlayClickSound(); StartButtonFunction(); });
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

        slider_length.minValue = 10;
        slider_length.maxValue = 100;
        slider_length.wholeNumbers = true;
        slider_length.value = length;

        slider_curviness.minValue = 0f;
        slider_curviness.maxValue = 20f;
        slider_curviness.value = curviness;

        slider_difficulty.minValue = 1f;
        slider_difficulty.maxValue = 5f;
        slider_difficulty.wholeNumbers = true;
        slider_difficulty.value = diffficulty;

        inputField_seed.ActivateInputField();

        base.Show();

        switchCanvas(ActiveCanvas.GAMEMODE);
    }

    public override void Hide()
    {
        KillLobbySearcherThread();

        base.Hide();
    }

    private void switchCanvas(ActiveCanvas canvas)
    {
        Hide();

        switch (canvas)
        {
            case ActiveCanvas.GAMEMODE:
                canvases[0].enabled = true;
                break;
            case ActiveCanvas.SINGLE:
                canvases[1].enabled = true;
                break;
            case ActiveCanvas.MULTI:
                canvases[2].enabled = true;
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
        switchCanvas(ActiveCanvas.SINGLE);
        
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Singleplayer();
    }

    public void MultiplayerButtonFunction()
    {
        switchCanvas(ActiveCanvas.MULTI);

        //are lobbies already searched?
        if (lobbySearcherThread != null && lobbySearcherThread.IsAlive)
            return;

        availableLobbies.Clear();

        //start a lobby searcher thread
        lobbySearcherThread = new Thread(SearchForAvailableLobbies);
        lobbySearcherThread.IsBackground = true;
        lobbySearcherThread.Start();
    }

    public void GoBackFromSingleButtonFunction()
    {
        switchCanvas(ActiveCanvas.GAMEMODE);

        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Gamemode();
    }

    public void StartButtonFunction()
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

    private void SearchForAvailableLobbies()
    {
        using (UdpClient client = new UdpClient(42069))//so that the socket is yeeted automatically
        {
            client.Client.ReceiveTimeout = 100;

            //TODO: send a request in every for example 5 seconds to get up-to-date lobby info
            byte[] joinMsg = Encoding.ASCII.GetBytes("yo i wanna join");
            client.Send(joinMsg, joinMsg.Length, new IPEndPoint(IPAddress.Broadcast, 42666));

            while (true)
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    byte[] reply=client.Receive(ref remoteEP);

                    //expects a 5-long string array
                    //replyMsg[0]: ip address
                    //replyMsg[1]: port
                    //replyMsg[2]: owner name
                    //replyMsg[3]: current player count
                    //replyMsg[4]: max player count
                    string[] replyMsg = Encoding.ASCII.GetString(reply).Split("||");

                    IPEndPoint serverEP=new IPEndPoint(long.Parse(replyMsg[0]),int.Parse(replyMsg[1]));
                    string ownerName = replyMsg[2];
                    int playerCount=int.Parse(replyMsg[3]);
                    int maxPlayerCount=int.Parse(replyMsg[4]);

                    availableLobbies.Add(new AvailableLobby(
                            serverEP,
                            ownerName,
                            playerCount,
                            maxPlayerCount
                        ));
                }
                catch(SocketException se)
                {
                    if (se.SocketErrorCode == SocketError.TimedOut)
                        continue;

                    Debug.Log(se.ToString());
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
    }
}
