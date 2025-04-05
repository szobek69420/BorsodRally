using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class GameManagerBase : MonoBehaviour
{
    public enum GameState
    {
        LOBBY,
        COUNTDOWN,
        RACE,
        END
    };

    private GameState _state;
    public GameState State
    {
        get { return _state; }
        protected set { _state = value; }
    }

    [SerializeField] protected RacetrackGenerator track;

    protected List<GameObject> players = new List<GameObject>();

    private void Start()
    {
        InitScene();
    }

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
}
