using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public abstract class RacerBase : MonoBehaviour
{
    protected GameManagerBase gameManager;

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

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("FinishLine"))
        {
            gameManager?.RegisterFinish(this.gameObject);
            RacerOnFinish();
        }
    }

    private void GetGameManager()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManagerBase>();
    }

    protected abstract void RacerUpdate();
    protected abstract void RacerFixedUpdate();
    protected abstract void RacerOnFinish();
}
