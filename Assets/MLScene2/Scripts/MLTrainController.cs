using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;

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

    [SerializeField] RacetrackGenerator track;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform raycastOrigin;

    [SerializeField] private bool phase1;
    [SerializeField] private bool phase2;
    [SerializeField] private bool phase3;
    [SerializeField] private bool phase4;

    public float SteerInput { get; private set; } = 0;
    public float AccelInput { get; private set; } = 0;
    public float BrakeInput { get; private set; } = 0;

    private float lastProgress = 0.0f;
    private float startTime = 0.0f;
    private float[] distances=null;
    private float[] normalizedAngles = null;

    private void FixedUpdate()
    {
        distances = Raycast();
        normalizedAngles = CalculateNormalizedAngles();

        //check if the car is falling
        if (Mathf.Abs(rb.velocity.y) > 10.0f)
        {
            Dieded(-2000.0f);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            EndEpisode();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer==7)//the car collided with the track walls
        {
            AddReward(-100.0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer==7)
        {
            if (phase1)
                Dieded(-2000.0f);
            else if (phase2)
                Dieded(0.0f);
            else if (phase3)
                AddReward(-500.0f);
        }
    }

    public override void OnEpisodeBegin()
    {
        //reset the track
        track.RandomizeParameters();
        track.ResetGen();

        lastProgress = 0.0f;
        startTime = Time.time;

        //set the position of the car
        Transform startLine=track.GetStartLine();
        transform.position = startLine.position + 3.0f * Vector3.up+Random.Range(-1.5f, 1.5f)*startLine.right;
        transform.rotation = Quaternion.Euler(
            startLine.rotation.eulerAngles.x,
            startLine.rotation.eulerAngles.y + Random.Range(-30.0f, 30.0f),
            startLine.rotation.eulerAngles.z); ;

        //reset velocity
        rb.velocity = 5.0f*transform.forward;
        rb.angularVelocity = Vector3.zero;
    }

    //if the local basis is rotated, it might not work
    public override void CollectObservations(VectorSensor sensor)
    {
        if (phase1 == true)//phase 1 of the training
        {
            /*
            in the 1st phase of the training only the wall distances are given a value
            the episode ends if the player touches the walls
            */

            Vector3 velocity = rb.velocity;
            float speed = velocity.magnitude;
            bool isStationary = speed < 0.01f;
            Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

            //wall distances
            if (distances == null)
                distances = Raycast();
            for(int i=0;i<distances.Length;i++)
            {
                if (i == distances.Length - 1)//the backwards direction should be ignored
                    sensor.AddObservation(1.0f);//1, not 0 because that would be a sudden change when starting to receive the actual values in phase2
                else
                    sensor.AddObservation(distances[i]/RAYCAST_MAX_DISTANCE);
            }

            //normalized angles are 0.5f
            sensor.AddObservation(0.5f);
            sensor.AddObservation(0.5f);

            //speed is 0
            sensor.AddObservation(0.0f);

            //tilt is 0
            sensor.AddObservation(0.0f);

            //reward the speed and punish standing in one place
            AddReward(0.5f*Vector3.Dot(rb.velocity, transform.forward) - 10.0f);
            if (speed < 3.0f)
                AddReward(-30.0f);

            //reward the progress
            float currentProgress = track.CalculateProgress(rb.position);
            if (lastProgress < currentProgress)
            {
                AddReward(10000.0f * (currentProgress - lastProgress));
                lastProgress = currentProgress;
            }
        }
        else if (phase2 == true)//phase 2 of the training
        {
            //the car should be faster here than in phase1

            Vector3 velocity = rb.velocity;
            float speed = velocity.magnitude;
            bool isStationary = speed < 0.01f;
            Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

            //wall distances
            if (distances == null)
                distances = Raycast();
            for (int i = 0; i < distances.Length; i++)
            {
                if (i == distances.Length - 1)//the backwards direction should be ignored
                    sensor.AddObservation(1.0f);//1, not 0 because that would be a sudden change when starting to receive the actual values in phase2
                else
                    sensor.AddObservation(distances[i] / RAYCAST_MAX_DISTANCE);
            }

            //normalized angles
            sensor.AddObservation(normalizedAngles[0]);
            sensor.AddObservation(0.5f);

            //speed is 0
            sensor.AddObservation(0.01f*speed);

            //tilt is 0
            sensor.AddObservation(0.0f);


            //reward the progress with discounting
            float currentProgress = track.CalculateProgress(rb.position);
            if (lastProgress < currentProgress)
            {
                AddReward((100000.0f/(1.0f+0.02f*(Time.time-startTime))) * (currentProgress - lastProgress));
                lastProgress = currentProgress;
            }
        }
        else if(phase3==true)//phase 3 of the training
        {
            //the car should be faster here than in phase1

            Vector3 velocity = rb.velocity;
            float speed = velocity.magnitude;
            bool isStationary = speed < 0.01f;
            Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

            //wall distances
            if (distances == null)
                distances = Raycast();
            for (int i = 0; i < distances.Length; i++)
            {
                if (i == distances.Length - 1)//the backwards direction should be ignored
                    sensor.AddObservation(1.0f);//1, not 0 because that would be a sudden change when starting to receive the actual values in phase2
                else
                    sensor.AddObservation(distances[i] / RAYCAST_MAX_DISTANCE);
            }

            //normalized angles
            sensor.AddObservation(normalizedAngles[0]);
            sensor.AddObservation(normalizedAngles[1]);

            //speed
            sensor.AddObservation(0.01f * speed);

            //tilt
            sensor.AddObservation(CalculateTilt());


            //reward the progress with discounting
            float currentProgress = track.CalculateProgress(rb.position);
            if (lastProgress < currentProgress)
            {
                AddReward((100000.0f / (1.0f + 0.02f * (Time.time - startTime))) * (currentProgress - lastProgress));
                lastProgress = currentProgress;
            }
        }
        else if (phase4 == true)//phase 4 of the training
        {
            /*
            in the 4th phase of the training the agent gets information about the direction it should be going towards and the speed at it is going
            */

            Vector3 velocity = rb.velocity;
            float speed = velocity.magnitude;
            bool isStationary = speed < 0.01f;
            Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

            //wall distances
            if (distances == null)
                distances = Raycast();
            for (int i = 0; i < distances.Length; i++)
            {
                if (i == distances.Length - 1)//the backwards direction should be divided by a different value
                    sensor.AddObservation(distances[i] / RAYCAST_MAX_DISTANCE_BACKWARDS);
                else
                    sensor.AddObservation(distances[i] / RAYCAST_MAX_DISTANCE);
            }

            //the angle between the velocity and some of the upcoming track points
            if (normalizedAngles == null)
                normalizedAngles = CalculateNormalizedAngles();
            sensor.AddObservation(normalizedAngles[0]);
            sensor.AddObservation(normalizedAngles[1]);

            //velocity
            sensor.AddObservation(0.01f * speed);

            //tilt
            sensor.AddObservation(CalculateTilt());

            //reward the progress
            float currentProgress = track.CalculateProgress(rb.position);
            if (lastProgress < currentProgress)
            {
                AddReward(10000.0f * (currentProgress - lastProgress));
                lastProgress = currentProgress;
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        SteerInput = actions.ContinuousActions[0];
        AccelInput = actions.ContinuousActions[1];
        BrakeInput = 0.333f * actions.DiscreteActions[0];
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxisRaw("Horizontal");
        continuous[1] = Input.GetAxisRaw("Vertical");

        ActionSegment<int> discrete = actionsOut.DiscreteActions;
        discrete[0] = Input.GetKey(KeyCode.Space) ? 3 : 0;
    }

    public void GoalReached()
    {
        EndEpisode();
    }

    public void Dieded(float reward)
    {
        AddReward(reward);
        EndEpisode();
    }

    private float[] Raycast()
    {
        float[] distances = new float[RAYCAST_DIRECTIONS.Length];

        int mask = LayerMask.GetMask("TrackWall");

        Vector3 forward = transform.forward;
        //if (Mathf.Pow(rb.velocity.x, 2.0f) + Mathf.Pow(rb.velocity.z, 2.0f) > 1.0f)
            //forward = rb.velocity.normalized;
        Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.up, forward));

        for (int i = 0; i < RAYCAST_DIRECTIONS.Length; i++)
        {
            Vector3 raycastDirection =
                RAYCAST_DIRECTIONS[i].x * right +
                RAYCAST_DIRECTIONS[i].z * forward;
            raycastDirection = Vector3.Normalize(new Vector3(raycastDirection.x, 0.0f, raycastDirection.z));

            if(i<RAYCAST_DIRECTIONS.Length-1)//forwards directions
            {
                RaycastHit hit;
                if (Physics.Raycast(
                    raycastOrigin.position,
                    raycastDirection,
                    out hit,
                    RAYCAST_MAX_DISTANCE,
                    mask)
                    )
                    distances[i] = hit.distance;
                else
                    distances[i] = RAYCAST_MAX_DISTANCE;
            }
            else//backwards direction
            {
                RaycastHit hit;
                if (Physics.Raycast(
                    raycastOrigin.position,
                    raycastDirection,
                    out hit,
                    RAYCAST_MAX_DISTANCE_BACKWARDS,
                    mask)
                    )
                    distances[i] = hit.distance;
                else
                    distances[i] = RAYCAST_MAX_DISTANCE_BACKWARDS;
            }


            //draw ray
            Debug.DrawLine(raycastOrigin.position, raycastOrigin.position+distances[i] * raycastDirection, Color.red, Time.fixedDeltaTime);
        }


        return distances;
    }

    //calculates the normalized tilt which is the angle between the velocity vector and the forward direction squeezed into [0;1]
    float CalculateTilt()
    {
        float tiltNormalized = 0.0f;

        if (rb.velocity.magnitude < 0.01f)
            return 0.5f;

        tiltNormalized = Mathf.Acos(Vector3.Dot(transform.forward, rb.velocity.normalized));
        if (Vector3.Dot(rb.velocity, transform.right) < 0.0f)
            tiltNormalized *= -1;
        tiltNormalized /= 0.5f * Mathf.PI;
        tiltNormalized = Mathf.Clamp(tiltNormalized, -1.0f, 1.0f);

        tiltNormalized = 0.5f * tiltNormalized + 0.5f;

        return tiltNormalized;
    }

    //the normalized angles between the velocity and some upcoming track points
    float[] CalculateNormalizedAngles()
    {
        Vector3 velocityNormalized;
        if(rb.velocity.sqrMagnitude<1.0f)
            velocityNormalized=transform.forward;
        else
            velocityNormalized = rb.velocity.normalized;
        Vector3 horizontalVelocityNormalized = new Vector3(velocityNormalized.x, 0.0f, velocityNormalized.z).normalized;

        int currentTrackPoint = track.GetNearestTrackPointIndex(rb.position-track.transform.position);

        int[] upcomingTrackPoints = new int[2];
        upcomingTrackPoints[0] = Mathf.Clamp(currentTrackPoint + 30, currentTrackPoint, track.TrackPoints.Count - 1);
        upcomingTrackPoints[1] = Mathf.Clamp(currentTrackPoint + 80, currentTrackPoint, track.TrackPoints.Count - 1);

        float[] normalizedAngles = new float[2];

        for (int i = 0; i < upcomingTrackPoints.Length; i++)
        {
            Vector3 trackPointDirection = track.TrackPoints[upcomingTrackPoints[i]] - track.TrackPoints[currentTrackPoint];
            trackPointDirection.y = 0.0f;
            trackPointDirection = Vector3.Normalize(trackPointDirection);

            float normalizedAngle = Mathf.Atan2(
                    Vector3.Dot(Vector3.Cross(Vector3.up, trackPointDirection), horizontalVelocityNormalized),
                    Vector3.Dot(trackPointDirection, horizontalVelocityNormalized)
                    );
            normalizedAngle /= 0.5f*Mathf.PI;
            normalizedAngle = 0.5f * Mathf.Clamp(normalizedAngle, -1.0f, 1.0f) + 0.5f;

            normalizedAngles[i] = normalizedAngle;

            //draw the direction to the track point
            Debug.DrawLine(raycastOrigin.position, track.gameObject.transform.position+track.TrackPoints[upcomingTrackPoints[i]], Color.green, Time.fixedDeltaTime);
        }

        return normalizedAngles;
    }
}
