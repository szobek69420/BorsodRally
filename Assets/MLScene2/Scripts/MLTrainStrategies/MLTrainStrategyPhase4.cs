using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MLTrainStrategyPhase4 : MLTrainStrategyBase
{
    private static object synchronizer=new object();
    //if an agent starts an episode, it increments the value, if an agent wants to stop an episode, it decrements the value
    private static int agentsInGame = 0;
    private static bool restartImpending = false;

    private int _agentCount = -1;
    private int AgentCount
    {
        get
        {
            if(_agentCount == -1)
                _agentCount = controller.transform.parent.GetComponentsInChildren<MLTrainStrategyPhase4>().Length;
            return _agentCount;
        }
    }

    private bool isAgentWaiting = false;//is the agent waiting for a new episode?


    protected override void OnFixedUpdate()
    {
        //check if the car is falling
        if (Mathf.Abs(controller.rb.velocity.y) > 10.0f)
        {
            EndEpisode(-2000.0f);
        }

        lock (synchronizer)
        {
            if (agentsInGame == 0)
            {
                restartImpending = true;
            }

            if (restartImpending)
            {
                controller.EndEpisode();
                if(agentsInGame == AgentCount)
                    restartImpending=false;
                Debug.Log("restart " + GetHashCode());
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        int prevAgentsInGame=0;

        lock (synchronizer)
        {
            prevAgentsInGame = agentsInGame;

            if(agentsInGame == 0)
            {
                //reset the track
                controller.track.RandomizeParameters();
                controller.track.ResetGen();
            }

            isAgentWaiting = false;
            agentsInGame++;
        }

        lastProgress = 0.0f;
        startTime = Time.time;

        //set the position of the car
        Transform startLine = controller.track.GetStartLine();
        transform.position = 
            startLine.position +
            3.0f * Vector3.up + 
            2.0f*(2*(prevAgentsInGame%2)-1) * startLine.right -
            6.0f*(prevAgentsInGame/2)*startLine.forward;

        transform.rotation = Quaternion.Euler(
            startLine.rotation.eulerAngles.x,
            startLine.rotation.eulerAngles.y + Random.Range(-30.0f, 30.0f),
            startLine.rotation.eulerAngles.z); ;

        //reset velocity
        controller.rb.velocity = 5.0f * transform.forward;
        controller.rb.angularVelocity = Vector3.zero;

        controller.rb.isKinematic = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        bool isAgentWaiting = false;
        lock (synchronizer)
            isAgentWaiting = this.isAgentWaiting;

        if(!isAgentWaiting)
        {
            Vector3 velocity = controller.rb.velocity;
            float speed = velocity.magnitude;
            bool isStationary = speed < 0.01f;
            Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

            //wall distances
            for (int i = 0; i < distances.Length; i++)
                sensor.AddObservation(distances[i]);

            //normalized angles
            sensor.AddObservation(normalizedAngles[0]);
            sensor.AddObservation(normalizedAngles[1]);

            //speed
            sensor.AddObservation(0.01f * Vector3.Dot(transform.forward, controller.rb.velocity) + 0.5f);

            //tilt
            sensor.AddObservation(tilt);

            //distance from next car is 1
            sensor.AddObservation(1.0f);

            //reward the progress with discounting
            float currentProgress = controller.track.CalculateProgress(controller.rb.position);
            if (lastProgress < currentProgress)
            {
                controller.AddReward((100000.0f / (1.0f + 0.02f * (Time.time - startTime))) * (currentProgress - lastProgress));
                lastProgress = currentProgress;
            }

            //punish being slow and reward being speedy
            if (Mathf.Abs(Vector3.Dot(transform.forward, controller.rb.velocity)) < 2.0f)
                controller.AddReward(Mathf.Abs(Vector3.Dot(transform.forward, controller.rb.velocity)) - 40.0f);
        }
        else
        {
            for (int i = 0; i < distances.Length; i++)
                sensor.AddObservation(1.0f);

            sensor.AddObservation(0.0f);
            sensor.AddObservation(0.0f);

            sensor.AddObservation(0.0f);
            sensor.AddObservation(0.0f);
        }
    }

    public override void OnOnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            EndEpisode(0.0f);
        }
        else if (other.gameObject.CompareTag("FinishLine"))
            EndEpisode(2000.0f);
    }

    private void EndEpisode(float reward)
    {
        lock (synchronizer)
        {
            isAgentWaiting = true;
            agentsInGame--;

            controller.AddReward(reward);

            controller.rb.isKinematic = true;
        }
    }
}
