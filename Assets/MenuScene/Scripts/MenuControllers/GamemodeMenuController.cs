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

    [SerializeField] private Canvas canvas_singlePlayer;
    [SerializeField] private Canvas canvas_multiPlayer;
    [SerializeField] private Canvas canvas_gameMode;

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
        
        slider_length.value = length;
        slider_curviness.value = curviness;
        slider_difficulty.value = diffficulty;

        inputField_seed.ActivateInputField();

        canvas_gameMode.enabled = true;
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
        canvas_gameMode.enabled = false;
        canvas_singlePlayer.enabled = true;
        
        GameObject.Find("MenuManager").GetComponent<MenuCameraPositions>().Singleplayer();
    }

    public void MultiplayerButtonFunction()
    {
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
        canvas_singlePlayer.enabled = false;
        canvas_gameMode.enabled = true;

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

                    string replyMsg = Encoding.ASCII.GetString(reply);
                    availableLobbies.Add(AvailableLobby.ParseString(replyMsg));
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
