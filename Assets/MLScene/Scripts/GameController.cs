using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private TrackManager trackManager;
    [SerializeField] private GameObject playerPrefab;

    private GameObject playerInstance = null;

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

    void ResetGame()
    {
        if(playerInstance != null)
            GameObject.Destroy(playerInstance);

        playerInstance = GameObject.Instantiate(playerPrefab, transform);
        trackManager.ResetTrack(playerInstance.transform);
        trackManager.StartGenerate();
    }
}
