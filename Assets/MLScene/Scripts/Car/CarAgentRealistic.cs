using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgentRealistic : Agent
{

    private RealisticCar car;
    private GameController controller;
    private TrackManager trackManager;

    private void Start()
    {

    }

    public override void OnEpisodeBegin()
    {
        car = GetComponent<RealisticCar>();
        controller = GetComponentInParent<GameController>();
        trackManager = controller.gameObject.GetComponent<TrackManager>();

        controller.ResetGame();
    }

    //if the local basis is rotated, it might not work
    public override void CollectObservations(VectorSensor sensor)
    {
        float speed = car.gameObject.GetComponent<Rigidbody>().velocity.magnitude;

        //wall distances
        for (int i = 0; i < car.distanceFromWall.Length; i++)
            sensor.AddObservation(car.distanceFromWall[i] / RealisticCar.RAYCAST_MAX_DISTANCE);

        //alignment to the next checkpoint

        float alignment = Mathf.Atan2(
            Vector3.Dot(Vector3.Normalize(trackManager.NextCheckpointPosition(car.gameObject.transform.localPosition) - transform.localPosition), transform.right),
            Vector3.Dot(Vector3.Normalize(trackManager.NextCheckpointPosition(car.gameObject.transform.localPosition) - transform.localPosition), transform.forward)
            );
        sensor.AddObservation(alignment / Mathf.PI);

        //velocity
        sensor.AddObservation(0.002f * speed);

        //tilt
        sensor.AddObservation(car.tiltNormalized);

        //add reward for distance from wall based on distance from wall while facing the right direction
        AddReward(0.01f*Mathf.Abs(car.tiltNormalized-0.5f)*car.distanceFromWall[0] * car.distanceFromWall[car.distanceFromWall.Length-1]);

        //punish slow driving
        if (speed < 30.0f)
            AddReward(0.02f*(speed-30.0f));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        car.SteerAngle = actions.ContinuousActions[0];

        if (actions.ContinuousActions[1] >= 0.0f)
        {
            car.Brake = 0.0f;
            car.Throttle = actions.ContinuousActions[1];
        }
        else
        {
            car.Throttle = 0.0f;
            car.Brake = -actions.ContinuousActions[1];
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Track"))
        {
            //punish the agent if the car is very fast on collision
            float currentReward = GetCumulativeReward();
            float markiplier = 1-(0.02f * car.gameObject.GetComponent<Rigidbody>().velocity.magnitude);
            SetReward(currentReward * markiplier);
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Checkpoint"))
        {
            AddReward(100.0f);
            trackManager?.CheckpointReached(other.gameObject);
        }
    }
}
