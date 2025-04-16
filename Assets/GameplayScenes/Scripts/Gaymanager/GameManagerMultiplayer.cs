using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerMultiplayer : GameManagerBase
{
	[SerializeField] private GameObject carPrefab_player;
	[SerializeField] private GameObject carPrefab_opponent;

	private GameManagerMultiplayerUIVariables ui = null;

	private List<PlayerInfo> joinedPlayers =new List<PlayerInfo>();

    //lobby things
    [SerializeField] private GameObject lobbyElement_prefab;
    private List<GameObject> instantiatedLobbyElements = new List<GameObject>();
	private JoinedPlayerInfo lobbyInfo = new JoinedPlayerInfo(69);	//the current state of the lobby
	private bool lobbyInfoUpdated = true;	//should reinstantiate the lobby elements (new state has arrived from the server)

	//countdown things

	//ingame things
	private float greatestProgress = 0;

	//end things

	public override void OnNetworkSpawn()
	{
		//get the ui elements
		ui=GameObject.Find("NetworkManager").GetComponent<GameManagerMultiplayerUIVariables>();

        //register the player
        int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
        string name = PlayerPrefs.GetString("name" + processId);
        RegisterPlayerServerRpc(new PlayerInfo(name, processId));

        //initialize scene
        InitScene();

        //start countdown
        ShowLobbyScreen();
    }

	public override void OnDestroy()
	{
		if (!IsOwner)
			return;

		KillLobbyResponderThread();//necessary for debugging, because the editor can't automatically close threads on stop
	}

	protected override void InitScene()
	{
		if (!IsOwner)
			return;

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
	}

	protected override void ShowLobbyScreen()
	{
		State = GameState.LOBBY;

        if (!IsOwner)
            return;
        ui.canvas_lobby.enabled = true;
	}

	protected override void UpdateLobbyScreen()
	{
        //sync the data with the other players
        if (IsHost)
        {
			JoinedPlayerInfo players;
			switch(joinedPlayers.Count) 
			{
				case 0:
					players = new JoinedPlayerInfo(69);
					break;
				case 1:
					players = new JoinedPlayerInfo(joinedPlayers[0]);
					break;
                case 2:
                    players = new JoinedPlayerInfo(joinedPlayers[0], joinedPlayers[1]);
                    break;
                case 3:
                    players = new JoinedPlayerInfo(joinedPlayers[0], joinedPlayers[1], joinedPlayers[2]);
                    break;
				default:
					players = new JoinedPlayerInfo(joinedPlayers[0], joinedPlayers[1], joinedPlayers[2], joinedPlayers[3]);
					break;
            }

			UpdateJoinedPlayersClientRpc(players);
        }

        if (!IsOwner)
            return;

        //set canvas
        if (lobbyInfoUpdated)
		{
			lobbyInfoUpdated = false;

			for(int i=0;i<instantiatedLobbyElements.Count;i++)
				Destroy(instantiatedLobbyElements[i]);
			instantiatedLobbyElements.Clear();

			for(int i=1;i<=lobbyInfo.playerCount;i++)
			{
				GameObject instance = Instantiate(lobbyElement_prefab, ui.lobbyElement_origin);
				instance.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, -170.0f * (i-1), 0.0f);
				switch(i)
				{
					case 1:
						instance.GetComponent<LobbyUIElementHandler>().SetInfo(lobbyInfo.player1.name.ToString(), 1);
						break;
                    case 2:
                        instance.GetComponent<LobbyUIElementHandler>().SetInfo(lobbyInfo.player2.name.ToString(), 2);
                        break;
                    case 3:
                        instance.GetComponent<LobbyUIElementHandler>().SetInfo(lobbyInfo.player3.name.ToString(), 3);
                        break;
                    default:
                        instance.GetComponent<LobbyUIElementHandler>().SetInfo(lobbyInfo.player4.name.ToString(), 4);
                        break;
                }
				instantiatedLobbyElements.Add(instance);
			}

            ui.text_waitingForHost.gameObject.SetActive(!IsHost);
            ui.button_startGame.gameObject.SetActive(IsHost);
        }
    }

	protected override void StartCountdown()
	{
        State = GameManagerBase.GameState.COUNTDOWN;

        if (!IsOwner)
			return;
	}

	protected override void UpdateCountdownScreen()
	{
		if (!IsOwner)
			return;
	}
	protected override void StartRace()
	{
        State = GameManagerBase.GameState.RACE;

        if (!IsOwner)
			return;
	}
	protected override void UpdateRaceScreen()
	{
		if (!IsOwner)
			return;
	}

	public override void EndRace()
	{
        State = GameManagerBase.GameState.END;

        if (!IsOwner)
			return;
	}

	protected override void UpdateEndScreen()
	{
		if (!IsOwner)
			return;
	}

	protected override void ReturnToMenu()
	{
		if (!IsOwner)
			return;

		SceneManager.LoadScene("MenuScene");
	}

    [ServerRpc(RequireOwnership = false)]
    private void RegisterPlayerServerRpc(PlayerInfo playerInfo)
	{
        if (joinedPlayers.Contains(playerInfo) == false)
			joinedPlayers.Add(playerInfo);
	}

	[ClientRpc]
	private void UpdateJoinedPlayersClientRpc(JoinedPlayerInfo playersInLobby)
	{
		//do something
		lobbyInfo = playersInLobby;
		lobbyInfoUpdated = true;
	}

	//lobby responder things----------------------------------------------------------------------------------------------
	//this is the thread that is responsibly for responding to the lobby searcher thread in the menu
	private const int RECEIVE_TIMEOUT = 500;
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
					client.Client.ReceiveTimeout = RECEIVE_TIMEOUT;
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
