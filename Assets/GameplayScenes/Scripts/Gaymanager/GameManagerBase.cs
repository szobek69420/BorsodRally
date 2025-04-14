using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using Unity.Netcode;

public abstract class GameManagerBase : NetworkBehaviour
{
    public enum GameState
    {
        NOTHING,
        LOBBY,
        COUNTDOWN,
        RACE,
        END
    };

    private GameState _state=GameState.NOTHING;
    public GameState State
    {
        get { return _state; }
        protected set { _state = value; }
    }

    protected RacetrackGenerator track;

    protected List<GameObject> players = new List<GameObject>();
    protected List<GameObject> finishedPlayers= new List<GameObject>();

    private void Update()
    {
        switch(State)
        {
            case GameState.LOBBY:
                UpdateLobbyScreen();
                break;

            case GameState.COUNTDOWN:
                UpdateCountdownScreen();
                break;

            case GameState.RACE:
                UpdateRaceScreen();
                break;

            case GameState.END:
                UpdateEndScreen();
                break;
        }
    }

    protected abstract void InitScene();
    protected abstract void ShowLobbyScreen();
    protected abstract void UpdateLobbyScreen();
    protected abstract void StartCountdown();
    protected abstract void UpdateCountdownScreen();
    protected abstract void StartRace();
    protected abstract void UpdateRaceScreen();
    public abstract void EndRace();
    protected abstract void UpdateEndScreen();
    protected abstract void ReturnToMenu();

    //provides an interface for the players to tell if they have finished
    public void RegisterFinish(GameObject player)
    {
        if(finishedPlayers.Contains(player)==false)
            finishedPlayers.Add(player);
    }

    protected void GetTrackManager()
    {
        track = GameObject.Find("TrackManager").GetComponent<RacetrackGenerator>();
    }
}
