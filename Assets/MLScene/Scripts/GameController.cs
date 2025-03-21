using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private TrackManager trackManager;
    [SerializeField] private GameObject playerInstance;

    // Start is called before the first frame update
    void Start()
    {
        ResetGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
            ResetGame();
    }

    public void ResetGame()
    {
        Rigidbody playerRB;
        if(playerInstance.TryGetComponent<Rigidbody>(out playerRB))
        {
            playerRB.velocity= Vector3.zero;
            playerRB.angularVelocity= Vector3.zero;
        }

        trackManager.ResetTrack(playerInstance.transform);
        trackManager.StartGenerate();
    }
}
