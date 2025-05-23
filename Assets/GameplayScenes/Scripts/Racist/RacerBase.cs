using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public abstract class RacerBase : MonoBehaviour
{
    [SerializeField] protected IngameCarComponents carComponents;
    [SerializeField] private Transform centerOfMass;

    protected GameManagerBase gameManager;
    protected RacetrackGenerator track;

    // Start is called before the first frame update
    void Start()
    {
        GetManagers();

        //set the center of mass
        if (carComponents.rb != null)
        {
            Vector3 localPos = carComponents.rb.gameObject.transform.InverseTransformPoint(centerOfMass.position);
            carComponents.rb.centerOfMass = localPos;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        RacerUpdate();
    }

    private void FixedUpdate()
    {
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

    private void GetManagers()
    {
        gameManager = GameObject.Find("GameManager")?.GetComponent<GameManagerBase>();
        track = GameObject.Find("TrackManager")?.GetComponent<RacetrackGenerator>();
    }

    protected abstract void RacerUpdate();
    protected abstract void RacerFixedUpdate();
    protected abstract void RacerOnFinish();

    public void RacerOnUpsideDown()
    {
        int respawnPointIndex = track.GetNearestTrackPointIndex(transform.position - track.transform.position);
        respawnPointIndex = respawnPointIndex > 25 ? respawnPointIndex - 10 : 15;

        carComponents.rb.velocity = Vector3.zero;
        carComponents.rb.angularVelocity = Vector3.zero;

        transform.position = track.TrackPoints[respawnPointIndex] + track.transform.position + 2.0f * Vector3.up;
        transform.rotation = Quaternion.LookRotation(track.TrackPoints[respawnPointIndex + 1] - track.TrackPoints[respawnPointIndex]);
    }
}
