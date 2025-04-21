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
	public static GameManagerMultiplayer Singleton { get; private set; } = null;

	[SerializeField] private GameObject carPrefab_playerHost;
	[SerializeField] private GameObject carPrefab_opponentHost;

    [SerializeField] private GameObject carPrefab_playerClient;
    [SerializeField] private GameObject carPrefab_opponentClient;

    private GameManagerMultiplayerUIVariables ui = null;

	private List<PlayerInfo> joinedPlayers =new List<PlayerInfo>();

    //lobby things
    [SerializeField] private GameObject lobbyElement_prefab;
    private List<GameObject> instantiatedLobbyElements = new List<GameObject>();
	private JoinedPlayerInfo lobbyInfo = new JoinedPlayerInfo(69);	//the current state of the lobby
	private bool lobbyInfoUpdated = true;   //should reinstantiate the lobby elements (new state has arrived from the server)

	//countdown things
	private float countdownTime = 0.0f;

	//ingame things
	private float greatestProgress = 0;

	//end things

	public override void OnNetworkSpawn()
	{
		if(IsHost)
		{
			if(Singleton!=null&&Singleton!=this)
			{
				Debug.Log("Multiple GameManagerMultiplayer instances are not allowed");
				Destroy(gameObject);
				return;
			}
			Singleton = this;
		}
		else
            GetSingletonServerRpc(GameObject.Find("NetworkManager").GetComponent<NetworkManager>().LocalClientId);

        //get the ui elements
        ui = GameObject.Find("NetworkManager").GetComponent<GameManagerMultiplayerUIVariables>();

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

		ui.button_startGame.onClick.RemoveAllListeners();
		ui.button_startGame.onClick.AddListener(() => { StartCountdown(); });

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
		if(IsHost)
		{
            StartCountdownClientRpc();
			SpawnCars();
        }

		KillLobbyResponderThread();
    }

	protected override void UpdateCountdownScreen()
	{
		if(IsHost)
		{
            countdownTime -= Time.deltaTime;
			UpdateCountdownScreenClientRpc(countdownTime);
			UpdateCarOrientations();
        }

		//update text
        ui.text_countdown.text = (((int)countdownTime) + 1).ToString();

		if (countdownTime <= 0)
			StartRace();
    }
	protected override void StartRace()
	{
		if(IsHost)
			StartRaceClientRpc();
	}
	protected override void UpdateRaceScreen()
	{
		if(IsHost)
            UpdateCarOrientations();

		int processId=System.Diagnostics.Process.GetCurrentProcess().Id;
		foreach(GameObject player in players)
		{
			if(player.GetComponent<RacerId>().id == processId)
			{
				RacerPlayerMultiplayerClient rpmc;
				RacerPlayerMultiplayerHost rpmh;
				if (player.TryGetComponent<RacerPlayerMultiplayerClient>(out rpmc))
					ui.speedo.SetSpeed(rpmc.Velocity.magnitude);
				else if (player.TryGetComponent<RacerPlayerMultiplayerHost>(out rpmh))
					ui.speedo.SetSpeed(player.GetComponent<Rigidbody>().velocity.magnitude);
				else
					continue;
				break;
			}
		}
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
	private void GetSingletonServerRpc(ulong senderClientId)
	{
		if(Singleton!=null)
		{
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { senderClientId }
                }
            };
            SendSingletonClientRpc(NetworkObjectId, clientRpcParams);
        }
	}

	[ClientRpc]
	private void SendSingletonClientRpc(ulong networkObjectId, ClientRpcParams rpcParams)
	{
        if (Singleton == null)
        {
            if (GameObject.Find("NetworkManager").GetComponent<NetworkManager>().SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject networkObject))
            {
                Singleton = networkObject.GetComponent<GameManagerMultiplayer>();
				Singleton.gameObject.name = "GameManager";
                Debug.Log("Client: Singleton set");
            }
            else
                Debug.LogError("Client: NetworkObject with the id: " + networkObjectId + " isn't found");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterPlayerServerRpc(PlayerInfo playerInfo)
	{
        if (joinedPlayers.Contains(playerInfo) == false)
			joinedPlayers.Add(playerInfo);
	}

	[ClientRpc(RequireOwnership = false)]
	private void UpdateJoinedPlayersClientRpc(JoinedPlayerInfo playersInLobby)
	{
		//do something
		lobbyInfo = playersInLobby;
		lobbyInfoUpdated = true;
	}

	[ClientRpc(RequireOwnership = false)]
	private void StartCountdownClientRpc()
	{
        State = GameManagerBase.GameState.COUNTDOWN;

        countdownTime = 3.0f;

		ui.canvas_lobby.enabled = false;
        ui.canvas_countdown.enabled = true;
    }

	[ClientRpc(RequireOwnership = false)]
	private void UpdateCountdownScreenClientRpc(float countdownTime)
	{
		this.countdownTime = countdownTime;
	}

	[ClientRpc(RequireOwnership = false)]
	private void StartRaceClientRpc()
	{
        State = GameManagerBase.GameState.RACE;

		ui.canvas_countdown.enabled = false;
		ui.canvas_ingame.enabled = true;
    }

	public void UpdateClientInput(CarInput input)
	{
		UpdateClientInputServerRpc(input);
	}

	[ServerRpc(RequireOwnership = false)]
	private void UpdateClientInputServerRpc(CarInput input)
	{
		foreach(GameObject player in players)
		{
			if(player.GetComponent<RacerId>().id==input.id)
			{
				IngameCarController cc=player.GetComponent<IngameCarController>();

				cc.AccelInput = input.throttleInput;
				cc.BrakeInput = input.brakeInput;
				cc.SteerInput = input.steerInput;

				break;
			}
		}
	}

	private void SpawnCars()
	{
		if (!IsHost)
			return;

        //instantiate racers
        Transform startLine = track.GetStartLine();
		int processId=System.Diagnostics.Process.GetCurrentProcess().Id;

		for(int i=0;i<joinedPlayers.Count;i++)
		{
			int x = 2*(i % 2)-1;
			int y = 2*(i / 2)-1;

            Vector3 spawnPosition = startLine.position + 3 * x * startLine.right + 5 * y * startLine.forward + 2.0f * startLine.up;
            GameObject racer = null;
            if (joinedPlayers[i].id==processId)
                racer = GameObject.Instantiate(carPrefab_playerHost, spawnPosition, startLine.rotation);
            else
                racer = GameObject.Instantiate(carPrefab_opponentHost, spawnPosition, startLine.rotation);
			racer.GetComponent<RacerId>().id=joinedPlayers[i].id;

            players.Add(racer);
        }

		//send the data to the clients
        UpdateCarOrientations();
	}

	private void UpdateCarOrientations()
	{
		List<CarOrientation> orientations = new List<CarOrientation>();

		for(int i=0;i<joinedPlayers.Count&&i<players.Count;i++)
		{
			CarOrientation co = players[i].GetComponent<IngameCarComponents>().GetOrientation();
			co.id = joinedPlayers[i].id;
			orientations.Add(co);
		}

		switch(orientations.Count)
		{
			case 0:
				return;
			case 1:
				UpdateCarOrientationsClientRpc(new CarOrientationPackage(orientations[0]));
				break;
			case 2:
                UpdateCarOrientationsClientRpc(new CarOrientationPackage(orientations[0], orientations[1]));
                break;
            case 3:
                UpdateCarOrientationsClientRpc(new CarOrientationPackage(orientations[0], orientations[1], orientations[2]));
                break;
            case 4:
                UpdateCarOrientationsClientRpc(new CarOrientationPackage(orientations[0], orientations[1], orientations[2], orientations[3]));
                break;
        }
	}

	[ClientRpc(RequireOwnership = false)]
	private void UpdateCarOrientationsClientRpc(CarOrientationPackage orientations)
	{
		if (IsHost)
			return;

		CarOrientation[] cos = new CarOrientation[] { orientations.car1, orientations.car2, orientations.car3, orientations.car4 };
		for (int i = 0; i < orientations.carCount;i++)
		{
			bool found = false;
			for (int j = 0; j < players.Count; j++)
			{
				if (players[j].GetComponent<RacerId>()?.id == cos[i].id)
				{
					found = true;

					//set orientation
					players[j].GetComponent<IngameCarComponents>().SetCurrentOrientation(cos[i]);
				}
			}

			//if not found, instantiate a new car
			if(!found)
			{
				int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
				GameObject racist = null;

				if (processId == cos[i].id)
				{
                    racist = Instantiate(carPrefab_playerClient);
					racist.GetComponent<RacerPlayerMultiplayerClient>().Velocity = new Vector3(cos[i].velocityX, cos[i].velocityY, cos[i].velocityZ);//set velocity
                }
				else
					racist = Instantiate(carPrefab_opponentClient);

				racist.GetComponent<RacerId>().id = cos[i].id;//set id
				racist.GetComponent<IngameCarComponents>().SetOrientation(cos[i]);//set orientation

				players.Add(racist);
			}
		}
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
