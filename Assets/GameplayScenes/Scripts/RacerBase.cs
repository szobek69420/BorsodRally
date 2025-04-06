using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public abstract class RacerBase : MonoBehaviour
{
    [SerializeField] protected IngameCarController carController;
    private GameManagerBase gameManager;

    // Start is called before the first frame update
    void Start()
    {
        GetGameManager();
    }

    // Update is called once per frame
    private void Update()
    {
        if (gameManager?.State == GameManagerBase.GameState.RACE)
            RacerUpdate();
    }

    private void FixedUpdate()
    {
        if (gameManager?.State == GameManagerBase.GameState.RACE)
            RacerFixedUpdate();
    }

    private void GetGameManager()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManagerBase>();
    }

    protected abstract void RacerUpdate();
    protected abstract void RacerFixedUpdate();
}
