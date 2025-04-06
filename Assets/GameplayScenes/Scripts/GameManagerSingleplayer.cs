using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManagerSingleplayer : GameManagerBase
{
    [SerializeField] private GameObject carPrefab_player;
    [SerializeField] private GameObject carPrefab_ai;

    //countdown things
    [SerializeField] private Canvas canvas_countdown;
    [SerializeField] private TMP_Text text_countdown;

    private float countdownTime = 3.0f;

    //ingame things
    [SerializeField] private Canvas canvas_ingame;

    //end things
    [SerializeField] private Canvas canvas_end;

    protected override void InitScene()
    {
        //generate track
        track.FetchParameters();
        track.StartGen();

        //instantiate racers (only the player yet)
        GameObject player=GameObject.Instantiate(carPrefab_player, new Vector3(0.0f, 2.0f, 0.0f), Quaternion.identity);
        players.Add(player);

        //start countdown
        StartCountdown();
    }

    protected override void ShowLobbyScreen()
    {
        //the singleplayer mode doesn't need a lobby screen
    }

    protected override void UpdateLobbyScreen()
    {
        //the singleplayer mode doesn't need a lobby screen
    }

    protected override void StartCountdown()
    {
        State=GameManagerBase.GameState.COUNTDOWN;
        countdownTime = 3.0f;
        canvas_countdown.enabled = true;
    }

    protected override void UpdateCountdownScreen()
    {
        countdownTime-=Time.deltaTime;

        if (countdownTime < 0)
            StartRace();
        else
            text_countdown.text = (((int)countdownTime) + 1).ToString();
    }
    protected override void StartRace()
    {
        State = GameManagerBase.GameState.RACE;

        canvas_countdown.enabled = false;
        canvas_ingame.enabled = true;
    }
    protected override void UpdateRaceScreen()
    {

    }

    public override void EndRace()
    {
        State = GameManagerBase.GameState.END;

        canvas_ingame.enabled=false;
        canvas_end.enabled = true;
    }

    protected override void UpdateEndScreen()
    {

    }
}
