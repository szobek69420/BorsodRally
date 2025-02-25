using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgentFullGrip : Agent
{
    
    private FullGripCar car;
    private GameController controller;

    private void Start()
    {
        
    }

    public override void OnEpisodeBegin()
    {
        car = GetComponent<FullGripCar>();
        controller = GetComponentInParent<GameController>();
        controller.ResetGame();
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        for(int i=0;i<car.distanceFromWall.Length;i++)
            sensor.AddObservation(car.distanceFromWall[i]/FullGripCar.RAYCAST_MAX_DISTANCE);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        car.SteerAngle = actions.ContinuousActions[0];

        if (actions.ContinuousActions[1]>=0.0f)
        {
            car.Brake = 0.0f;
            car.Throttle=actions.ContinuousActions[1];
        }
        else
        {
            car.Throttle = 0.0f;
            car.Brake = -actions.ContinuousActions[1];
        }

        //Debug.Log(actions.ContinuousActions[0] + " " + actions.ContinuousActions[1]);
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
            AddReward(-5.0f);
            EndEpisode();
        }
        else if (other.gameObject.CompareTag("Checkpoint"))
        {
            AddReward(1.0f);
            other.gameObject.SetActive(false);
        }
    }
}
