using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private SpeedometerHandler speedo;
    [SerializeField] private TMP_Text text_progress;

    private int highestIndexReached = 0;

    //end things
    [SerializeField] private Canvas canvas_end;
    [SerializeField] private TMP_Text text_position;
    [SerializeField] private Button button_returnToMenu;


    protected override void InitScene()
    {
        //generate track
        track.FetchParameters();
        track.StartGen();

        //instantiate racers (only the player yet)
        Transform startLine = track.GetStartLine();

        GameObject player=GameObject.Instantiate(carPrefab_player, new Vector3(0.0f, 2.0f, 0.0f), Quaternion.identity);
        player.transform.position = startLine.position + new Vector3(0.0f, 2.0f, 0.0f);
        player.transform.rotation = startLine.rotation;
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
        //get the player
        foreach(var player in players)
        {
            RacerPlayer rp = null;
            if (player.TryGetComponent<RacerPlayer>(out rp) == false)
                continue;

            //set the speedo
            Rigidbody rb = rp.gameObject.GetComponent<Rigidbody>();
            speedo.SetSpeed(rb.velocity.magnitude);

            //set the progress
            List<Vector3> trackPoints = track.TrackPoints;
            int closestIndex = -1;
            float closestSqrDistance = 0.0f;
            for(int i=0;i<trackPoints.Count;i++)
            {
                float currentSqrDistance=(player.transform.position-trackPoints[i]).sqrMagnitude;
                if(closestIndex==-1||currentSqrDistance<closestSqrDistance)
                {
                    closestIndex = i;
                    closestSqrDistance = currentSqrDistance;
                }
            }
            if(highestIndexReached < closestIndex)
                highestIndexReached = closestIndex;

            text_progress.text = (int)(100.0f * ((float)(highestIndexReached + 1) / trackPoints.Count)) + "%";
            break;
        }
    }

    public override void EndRace()
    {
        State = GameManagerBase.GameState.END;

        switch(finishedPlayers.Count)
        {
            case 1: text_position.text = "First";  break;
            case 2: text_position.text = "Second"; break;
            case 3: text_position.text = "Third"; break;
            case 4: text_position.text = "Fourth"; break;
            default: text_position.text=finishedPlayers.Count.ToString(); break;
        }

        button_returnToMenu.onClick.RemoveAllListeners();
        button_returnToMenu.onClick.AddListener(() => { ReturnToMenu(); });

        canvas_ingame.enabled=false;
        canvas_end.enabled = true;
    }

    protected override void UpdateEndScreen()
    {

    }

    protected override void ReturnToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
