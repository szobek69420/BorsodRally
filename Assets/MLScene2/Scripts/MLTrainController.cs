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
        new Vector3(Mathf.Sin(Mathf.PI), 0.0f, Mathf.Cos(Mathf.PI)) //backwards
    };
    private static float RAYCAST_MAX_DISTANCE = 150.0f;

    private RacetrackGenerator track = null;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform raycastOrigin;

    public float SteerInput { get; private set; } = 0;
    public float AccelInput { get; private set; } = 0;
    public float BrakeInput { get; private set; } = 0;

    private float lastProgress = 0.0f;
    private float[] distances=null;
    private float[] normalizedAngles = null;

    private void FixedUpdate()
    {
        distances = Raycast();
        normalizedAngles = CalculateNormalizedAngles();

        //check if the car is falling
        if (Mathf.Abs(rb.velocity.y) > 20.0f)
        {
            Dieded();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer==7)//the car collided with the track walls
        {
            AddReward(-10.0f);
        }
    }

    public override void OnEpisodeBegin()
    {
        //get the track generator
        if (!GameObject.Find("TrackManager").TryGetComponent<RacetrackGenerator>(out track))
            Debug.LogError("Couldn't find the RacetrackGenerator");

        //reset the track
        track.RandomizeParameters();
        track.ResetGen();

        lastProgress = 0.0f;

        //set the position of the car
        Transform startLine=track.GetStartLine();
        transform.position = startLine.position + 3.0f * Vector3.up;
        transform.rotation = startLine.rotation;

        //reset velocity
        rb.velocity = 20.0f*transform.forward;
        rb.angularVelocity = Vector3.zero;
    }

    //if the local basis is rotated, it might not work
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 velocity = rb.velocity;
        float speed = velocity.magnitude;
        bool isStationary = speed < 0.01f;
        Vector3 velocityNormalized = isStationary ? Vector3.zero : Vector3.Normalize(velocity);

        //wall distances
        if (distances == null)
            distances = Raycast();
        foreach (float distance in distances)
            sensor.AddObservation(distance / RAYCAST_MAX_DISTANCE);

        //the angle between the velocity and some of the upcoming track points
        if (normalizedAngles == null)
            normalizedAngles = CalculateNormalizedAngles();
        for(int i=0;i<normalizedAngles.Length;i++)
        {
            sensor.AddObservation(1.0f-normalizedAngles[i]);
        }

        //velocity
        sensor.AddObservation(0.002f * speed);

        //tilt
        sensor.AddObservation(CalculateTilt());

        //add reward for going in the right direction
        AddReward(100.0f*speed * (0.5f - Mathf.Abs(normalizedAngles[0] - 0.5f)));

        //reward the progress
        float currentProgress = track.CalculateProgress(rb.position);
        if(lastProgress< currentProgress)
        {
            AddReward(10000.0f * (currentProgress - lastProgress));
            lastProgress = currentProgress;
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        SteerInput = actions.ContinuousActions[0];
        AccelInput = actions.ContinuousActions[1];
        BrakeInput = 0.2f * actions.DiscreteActions[0];

        AddReward(-BrakeInput);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxisRaw("Horizontal");
        continuous[1] = Input.GetAxisRaw("Vertical");

        ActionSegment<int> discrete = actionsOut.DiscreteActions;
        discrete[0] = Input.GetKey(KeyCode.Space) ? 5 : 0;
    }

    public void GoalReached()
    {
        EndEpisode();
    }

    public void Dieded()
    {
        AddReward(-1000.0f);
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
            RaycastHit hit;
            if (Physics.Raycast(
                raycastOrigin.position,
                raycastDirection,
                out hit,
                RAYCAST_MAX_DISTANCE,
                mask))
            {
                distances[i] = hit.distance;
            }
            else
            {
                distances[i] = RAYCAST_MAX_DISTANCE;
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
        Vector3 velocityNormalized = rb.velocity.normalized;
        Vector3 horizontalVelocityNormalized = new Vector3(velocityNormalized.x, 0.0f, velocityNormalized.z).normalized;

        int currentTrackPoint = track.GetNearestTrackPointIndex(rb.position);

        int[] upcomingTrackPoints = new int[2];
        upcomingTrackPoints[0] = Mathf.Clamp(currentTrackPoint + 40, currentTrackPoint, track.TrackPoints.Count - 1);
        upcomingTrackPoints[1] = Mathf.Clamp(currentTrackPoint + 80, currentTrackPoint, track.TrackPoints.Count - 1);

        float[] normalizedAngles = new float[2];

        for (int i = 0; i < upcomingTrackPoints.Length; i++)
        {
            Vector3 trackPointDirection = track.TrackPoints[upcomingTrackPoints[i]] - track.TrackPoints[currentTrackPoint];
            trackPointDirection = Vector3.Normalize(trackPointDirection);

            float normalizedAngle = Mathf.Atan2(
                    Vector3.Dot(trackPointDirection, horizontalVelocityNormalized),
                    Vector3.Dot(Vector3.Cross(Vector3.up, trackPointDirection), horizontalVelocityNormalized)
                    );
            normalizedAngle /= Mathf.PI;
            normalizedAngle = 0.5f * normalizedAngle + 0.5f;
            normalizedAngle = Mathf.Clamp(normalizedAngle, 0.0f, 1.0f);

            normalizedAngles[i] = normalizedAngle;

            //draw the direction to the track point
            Debug.DrawLine(raycastOrigin.position, track.TrackPoints[upcomingTrackPoints[i]], Color.green, Time.fixedDeltaTime);
        }

        return normalizedAngles;
    }
}
