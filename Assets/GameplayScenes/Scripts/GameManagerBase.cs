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

    [SerializeField] private RacetrackGenerator track;

    protected List<RacerBase> players = new List<RacerBase>();

    protected abstract void InitScene();
    protected abstract void ShowLobbyScreen();
    protected abstract void UpdateLobbyScreen();
    protected abstract void StartCountdown();
    protected abstract void UpdateCountdownScreen();
    protected abstract void StartRace();
    protected abstract void UpdateRaceScreen();
    public abstract void EndRace();
    protected abstract void ShowEndScreen();
    protected abstract void UpdateEndScreen();
}
