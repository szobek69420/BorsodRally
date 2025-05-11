using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class MLTrainStrategyPhase3 : MLTrainStrategyBase
{
    public override void OnEpisodeBegin()
    {
        //reset the track
        controller.track.RandomizeParameters();
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

        //normalized angles
        sensor.AddObservation(normalizedAngles[0]);
        sensor.AddObservation(normalizedAngles[1]);

        //speed
        sensor.AddObservation(0.01f * Vector3.Dot(transform.forward, controller.rb.velocity) + 0.5f);

        //tilt
        sensor.AddObservation(tilt);


        //reward the progress with discounting
        float currentProgress = controller.track.CalculateProgress(controller.rb.position);
        if (lastProgress < currentProgress)
        {
            controller.AddReward((100000.0f / (1.0f + 0.02f * (Time.time - startTime))) * (currentProgress - lastProgress));
            lastProgress = currentProgress;
        }

        //reward being fast
        controller.AddReward(Mathf.Pow(0.05f * speed, 2.0f));
    }

    public override void OnActionReceived(ActionBuffers actions, out float steerInput, out float accelInput, out float brakeInput)
    {
        steerInput = actions.ContinuousActions[0];

        switch (actions.DiscreteActions[0])
        {
            case 0:
                if (Vector3.Dot(controller.rb.velocity, transform.forward) > 1.0f)//going forwards
                {
                    accelInput = 0.0f;
                    brakeInput = 1.0f;
                }
                else //going backwards
                {
                    accelInput = -1.0f;
                    brakeInput = 0.0f;
                }
                break;

            default:
                accelInput = 1.0f;
                brakeInput = 0.0f;
                break;
        }
    }

    public override void OnOnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
            controller.AddReward(-500.0f);
    }
}
