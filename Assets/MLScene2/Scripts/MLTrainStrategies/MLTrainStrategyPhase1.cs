using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.EventSystems;

public class MLTrainStrategyPhase1 : MLTrainStrategyBase
{
    protected override void OnFixedUpdate()
    {
        //check if the car is falling
        if (Mathf.Abs(controller.rb.velocity.y) > 10.0f)
        {
            controller.Dieded(-2000.0f);
        }
    }

    public override void OnEpisodeBegin()
    {
        //reset the track
        controller.track.RandomizeParameters(5.0f, 10.0f);
        controller.track.ResetGen();

        lastProgress = 0.0f;
        startTime = Time.time;

        //set the position of the car
        Transform startLine = controller.track.GetStartLine();
        transform.position = startLine.position + 3.0f * Vector3.up + Random.Range(-1.5f, 1.5f) * startLine.right;
        transform.rotation = Quaternion.Euler(
            startLine.rotation.eulerAngles.x,
            startLine.rotation.eulerAngles.y + Random.Range(-30.0f, 30.0f),
            startLine.rotation.eulerAngles.z); ;

        //reset velocity
        controller.rb.velocity = 5.0f * transform.forward;
        controller.rb.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*
            in the 1st phase of the training only the wall distances are given a value
            the episode ends if the player touches the walls
            */

        Vector3 velocity = controller.rb.velocity;
        float speed = velocity.magnitude;
        bool isStationary = speed < 0.01f;
        Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

        //wall distances
        for (int i = 0; i < distances.Length; i++)
        {
            if (i == distances.Length - 1)//the backwards direction should be ignored
                sensor.AddObservation(1.0f);//1, not 0 because that would be a sudden change when starting to receive the actual values in phase2
            else
                sensor.AddObservation(distances[i]);
        }

        //normalized angles are 0.5f
        sensor.AddObservation(0.5f);
        sensor.AddObservation(0.5f);

        //speed is 0
        sensor.AddObservation(0.0f);

        //tilt is 0
        sensor.AddObservation(0.0f);

        //distance from next car is 1
        sensor.AddObservation(1.0f);

        //reward the speed and punish standing in one place
        controller.AddReward(0.5f * Vector3.Dot(controller.rb.velocity, transform.forward) - 10.0f);
        if (speed < 3.0f)
            controller.AddReward(-30.0f);

        //reward the progress
        float currentProgress = controller.track.CalculateProgress(controller.rb.position);
        if (lastProgress < currentProgress)
        {
            controller.AddReward(10000.0f * (currentProgress - lastProgress));
            lastProgress = currentProgress;
        }
    }

    public override void OnOnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
            controller.Dieded(-2000.0f);
        else if (other.gameObject.CompareTag("FinishLine"))
            controller.Dieded(2000.0f);
    }
}
