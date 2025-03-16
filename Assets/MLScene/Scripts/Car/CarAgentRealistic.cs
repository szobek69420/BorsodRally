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
        Vector3 velocity = car.gameObject.GetComponent<Rigidbody>().velocity;
        float speed = velocity.magnitude;
        bool isStationary = speed < 0.01f;
        Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

        //wall distances
        for (int i = 0; i < car.distanceFromWall.Length; i++)
            sensor.AddObservation(car.distanceFromWall[i] / RealisticCar.RAYCAST_MAX_DISTANCE);

        //alignment to the next checkpoint
        float alignment = 0.0f;
        if(!isStationary)
        {
            alignment=Mathf.Atan2(
                Vector3.Dot(Vector3.Normalize(trackManager.NextCheckpointPosition(car.gameObject.transform.localPosition) - transform.localPosition), transform.right),
                Vector3.Dot(Vector3.Normalize(trackManager.NextCheckpointPosition(car.gameObject.transform.localPosition) - transform.localPosition), transform.forward)
                );
        }
        alignment /= Mathf.PI;
        alignment = 0.5f * alignment + 0.5f;
        alignment = Mathf.Clamp(alignment, 0.0f, 1.0f);
        sensor.AddObservation(alignment / Mathf.PI);

        //velocity
        sensor.AddObservation(0.002f * speed);

        //tilt
        sensor.AddObservation(car.tiltNormalized);

        //add reward for going in the right direction at high speed
        AddReward(0.01f*(0.5f-Mathf.Abs(alignment-0.5f)));

        //punish slow driving
        if (speed < 30.0f)
            AddReward(0.02f*(speed-30.0f));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        car.SteerAngle = actions.ContinuousActions[0];

        car.Throttle = actions.ContinuousActions[1];

        car.Brake = 0.2f * actions.DiscreteActions[0];

        AddReward(-car.Brake);
    }

    /*float lastBrakeForce = 0.0f;
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.Space))
            lastBrakeForce = Mathf.Lerp(lastBrakeForce, 1.0f, 0.1f);
        else
            lastBrakeForce = Mathf.Lerp(lastBrakeForce, 0.0f, 0.1f);
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = (int)(6 * lastBrakeForce);
    }*/

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
