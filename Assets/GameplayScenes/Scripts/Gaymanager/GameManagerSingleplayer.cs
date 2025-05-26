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

    private float greatestProgress = 0;

    //end things
    [SerializeField] private Canvas canvas_end;
    [SerializeField] private TMP_Text text_position;
    [SerializeField] private Button button_returnToMenu;

    private void Start()
    {
        InitScene();
    }

    protected override void InitScene()
    {
        //get the track generator
        GetTrackManager();

        //generate track
        track.FetchParameters();
        track.StartGen();

        //instantiate racers
        Transform startLine = track.GetStartLine();

        for(int i=-1;i<=1;i+=2)
        {
            for(int j=-1;j<=1;j+=2)
            {
                Vector3 spawnPosition = startLine.position + 3 * i * startLine.right + 5 * j * startLine.forward + 2.0f * startLine.up;
                GameObject racer = null;
                if(i==-1&&j==-1)
                    racer = GameObject.Instantiate(carPrefab_player, transform);
                else
                    racer = GameObject.Instantiate(carPrefab_ai, transform);

                //get the actual spawn position
                RaycastHit hit;
                if(Physics.Raycast(spawnPosition, Vector3.down, out hit, 20.0f, LayerMask.GetMask("Track")))
                    spawnPosition = hit.point + 0.15f * Vector3.up;

                racer.transform.position = spawnPosition;
                racer.transform.rotation = startLine.rotation;

                //set the racer to kinematic
                Rigidbody rb =racer.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = racer.GetComponentInChildren<Rigidbody>();

                if(rb!=null)
                    rb.isKinematic = true;

                //register the racer
                players.Add(racer);
            }
        }

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

        //set the racers to non kinematic
        foreach(GameObject player in players)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb == null)
                rb = player.GetComponentInChildren<Rigidbody>();

            if (rb != null)
                rb.isKinematic = false;
        }

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
            float currentProgress = track.CalculateProgress(rp.gameObject.transform.position);
            if(greatestProgress < currentProgress)
                greatestProgress = currentProgress;

            text_progress.text = (int)(100.0f * greatestProgress) + "%";
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
