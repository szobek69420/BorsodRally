using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

//the cars need to have a collider (preferably the bumper as it is the largest) of layer "Car" for the perception to work properly
//the car assumes that other cars are on the same level of the hierarchy
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

    private bool isAgentWaiting = true;//is the agent waiting for a new episode?


    private float otherCarDistance = 1.0f;

    protected override void OnFixedUpdate()
    {
        if (Mathf.Abs(controller.rb.velocity.y) > 10.0f)
        {
            Dieded(-2000.0f);
        }

        //gaycast
        otherCarDistance = CarRaycast();


        //check if the ai should be active or restarted
        lock (synchronizer)
        {
            if (agentsInGame == 0)
            {
                restartImpending = true;
            }

            if (restartImpending)
            {
                controller.EndEpisode();
                if (agentsInGame == AgentCount)
                    restartImpending = false;
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        int prevAgentsInGame=0;

        lock (synchronizer)
        {
            prevAgentsInGame = agentsInGame;

            if(isAgentWaiting) //i need to check this, because EndEpisode can be called on the max step count and then agentCount shouldn't be incremented
            {
                if (agentsInGame == 0)//if one of the agents didn't die through OnEndEpisode, the track will not be regenerated, but that's not a problem
                {
                    //reset the track
                    controller.track.RandomizeParameters(5.0f, 10.0f);
                    controller.track.ResetGen();
                }

                isAgentWaiting = false;
                agentsInGame++;
            }
            else
            {
                if (agentsInGame < AgentCount)
                    restartImpending = true;
            }
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

        //activate decision requester
        GetComponent<DecisionRequester>().enabled = true;
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

            //distance from next car
            sensor.AddObservation(otherCarDistance);

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
            sensor.AddObservation(1.0f);
        }
    }

    public override void OnOnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            Dieded(0.0f);
        }
        else if (other.gameObject.CompareTag("FinishLine"))
            Dieded(2000.0f);
    }

    public override void Dieded(float reward)
    {
        lock (synchronizer)
        {
            if (isAgentWaiting)
                return;

            isAgentWaiting = true;
            agentsInGame--;

            controller.AddReward(reward);
            controller.rb.velocity = Vector3.zero; //so that it won't be stuck at falling
            controller.rb.isKinematic = true;

            GetComponent<DecisionRequester>().enabled = false;
        }
    }

    //casts a ray forward and returns the normalized distance if another car has been hit, else returns 1
    private float CarRaycast()
    {
        Vector3 rayDirection = transform.forward;
        rayDirection.y = 0.0f;
        Vector3 raycastHit = Vector3.zero;
        MLTrainStrategyBase[] others = transform.parent.GetComponentsInChildren<MLTrainStrategyBase>();

        foreach (MLTrainStrategyBase other in others)
        {
            if (other.GetHashCode() == this.GetHashCode())
                continue;

            Vector3 rayOrigin = controller.raycastOrigin.transform.position;
            rayOrigin.y = other.GetController().raycastOrigin.transform.position.y;

            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, 30.0f, LayerMask.GetMask("Car"));

            for(int i = 0; i < hits.Length;i++)
            {
                if (hits[i].collider.attachedRigidbody == controller.rb)//the collider is part of the gaycaster car
                    continue;

                float prevDistance = Mathf.Sqrt(Mathf.Pow(raycastHit.x - controller.raycastOrigin.transform.position.x, 2.0f) + Mathf.Pow(raycastHit.z - controller.raycastOrigin.transform.position.z, 2.0f));
                if (raycastHit != Vector3.zero && hits[i].distance > prevDistance)
                    continue;

                raycastHit = hits[i].point;

                break;
            }
        }

        //draw ray
        if (raycastHit != Vector3.zero)
        { 
            Debug.DrawLine(controller.raycastOrigin.transform.position, raycastHit, new Color(0.0f, 1.0f, 1.0f), Time.fixedDeltaTime, false);
        }

        return raycastHit==Vector3.zero?
            1.0f:
            Mathf.Clamp(
                Mathf.Sqrt(
                    Mathf.Pow(raycastHit.x - controller.raycastOrigin.transform.position.x, 2.0f) 
                    + Mathf.Pow(raycastHit.z - controller.raycastOrigin.transform.position.z, 2.0f)
                    ) 
                / 30.0f,
                0.0f,
                1.0f
                );
    }
}
