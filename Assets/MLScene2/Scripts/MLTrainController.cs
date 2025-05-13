using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using System;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public class MLTrainController : Agent
{
    private static Vector3[] RAYCAST_DIRECTIONS = new Vector3[]
    {
        new Vector3(Mathf.Sin(-0.5f*Mathf.PI), 0.0f, Mathf.Cos(-0.5f*Mathf.PI)),
        new Vector3(Mathf.Sin(-0.25f*Mathf.PI), 0.0f, Mathf.Cos(-0.25f*Mathf.PI)),
        new Vector3(Mathf.Sin(0.0f*Mathf.PI), 0.0f, Mathf.Cos(0.0f*Mathf.PI)),
        new Vector3(Mathf.Sin(0.25f*Mathf.PI), 0.0f, Mathf.Cos(0.25f*Mathf.PI)),
        new Vector3(Mathf.Sin(0.5f*Mathf.PI), 0.0f, Mathf.Cos(0.5f*Mathf.PI)),
        new Vector3(Mathf.Sin(Mathf.PI), 0.0f, Mathf.Cos(Mathf.PI)) //backwards should be the last one
    };
    private static float RAYCAST_MAX_DISTANCE = 150.0f;
    private static float RAYCAST_MAX_DISTANCE_BACKWARDS = 10.0f;//short so that most of the time it doesn't interfere

    public RacetrackGenerator track;
    public Rigidbody rb;
    public Transform raycastOrigin;

    [SerializeField] private MLTrainStrategyBase strategyScript;
    private MLTrainStrategyBase strategy = null;

    public float SteerInput { get; private set; } = 0;
    public float AccelInput { get; private set; } = 0;
    public float BrakeInput { get; private set; } = 0;

    private void Start()
    {
        strategy = gameObject.AddComponent(strategyScript.GetType()) as MLTrainStrategyBase;
        strategy.SetController(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            EndEpisode();
    }

    private void OnTriggerEnter(Collider other)
    {
        strategy.OnOnTriggerEnter(other);
    }

    public override void OnEpisodeBegin()
    {
        strategy.OnEpisodeBegin();
    }

    //if the local basis is rotated, it might not work
    public override void CollectObservations(VectorSensor sensor)
    {
        strategy.CollectObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        SteerInput = actions.ContinuousActions[0];

        switch (actions.DiscreteActions[0])
        {
            case 0:
                if (Vector3.Dot(rb.velocity, transform.forward) > 1.0f)//going forwards
                {
                    AccelInput = 0.0f;
                    BrakeInput = 1.0f;
                }
                else //going backwards
                {
                    AccelInput = -1.0f;
                    BrakeInput = 0.0f;
                }
                break;

            default:
                AccelInput = 1.0f;
                BrakeInput = 0.0f;
                break;
        }
    }

    /*public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxisRaw("Horizontal");
        continuous[1] = Input.GetAxisRaw("Vertical");

        ActionSegment<int> discrete = actionsOut.DiscreteActions;
        discrete[0] = Input.GetKey(KeyCode.Space) ? 3 : 0;
    }*/

    public void Dieded(float reward)
    {
        AddReward(reward);
        EndEpisode();
    }
}
