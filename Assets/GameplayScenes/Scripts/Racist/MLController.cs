using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MLController : Agent
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
    private static float RAYCAST_MAX_DISTANCE_BACKWARDS = 15.0f;//short so that most of the time it doesn't interfere

    private float[] distances = null;
    private float[] normalizedAngles = null;
    private float tilt = 0.5f;
    private float otherCarDistance = 1.0f;

    private GameManagerBase gm = null;
	private RacetrackGenerator track = null;
	[SerializeField] private Rigidbody rb;
	[SerializeField] private Transform raycastOrigin;

	public float SteerInput { get; private set; } = 0;
	public float AccelInput{ get; private set; } = 0;
	public float BrakeInput { get; private set; } = 0;

    private void FixedUpdate()
    {
        distances = Raycast();
        normalizedAngles = CalculateNormalizedAngles();
        tilt = CalculateTilt();
        otherCarDistance = CarRaycast();
    }

    public override void OnEpisodeBegin()
	{
		//get game manager
		if(!GameObject.Find("GameManager").TryGetComponent<GameManagerBase>(out gm))
            Debug.LogError("Couldn't find the GameManager");

        //get the track generator
        if (!GameObject.Find("TrackManager").TryGetComponent<RacetrackGenerator>(out track))
            Debug.LogError("Couldn't find the RacetrackGenerator");
    }

	//if the local basis is rotated, it might not work
	public override void CollectObservations(VectorSensor sensor)
	{
        Vector3 velocity = rb.velocity;
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
        sensor.AddObservation(0.01f * Vector3.Dot(transform.forward, velocity) + 0.5f);

        //tilt
        sensor.AddObservation(tilt);

        //distance from next car is 1
        sensor.AddObservation(otherCarDistance);
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

    //functions for sensor values ------------------------------------------------------------------

    //returns values in [0;1]
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

            if (i < RAYCAST_DIRECTIONS.Length - 1)//forwards directions
            {
                RaycastHit hit;
                if (Physics.Raycast(
                    raycastOrigin.position,
                    raycastDirection,
                    out hit,
                    RAYCAST_MAX_DISTANCE,
                    mask)
                    )
                    distances[i] = hit.distance / RAYCAST_MAX_DISTANCE;
                else
                    distances[i] = 1.0f;

                //draw ray
                Debug.DrawLine(raycastOrigin.position, raycastOrigin.position + distances[i] * RAYCAST_MAX_DISTANCE * raycastDirection, Color.red, Time.fixedDeltaTime, false);
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
                    distances[i] = hit.distance / RAYCAST_MAX_DISTANCE_BACKWARDS;
                else
                    distances[i] = 1.0f;

                //draw ray
                Debug.DrawLine(raycastOrigin.position, raycastOrigin.position + distances[i] * RAYCAST_MAX_DISTANCE_BACKWARDS * raycastDirection, Color.red, Time.fixedDeltaTime, false);
            }
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
        if (rb.velocity.sqrMagnitude < 1.0f)
            velocityNormalized = transform.forward;
        else
            velocityNormalized = rb.velocity.normalized;
        Vector3 horizontalVelocityNormalized = new Vector3(velocityNormalized.x, 0.0f, velocityNormalized.z).normalized;

        int currentTrackPoint = track.GetNearestTrackPointIndex(rb.position - track.transform.position);

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
            normalizedAngle /= 0.5f * Mathf.PI;
            normalizedAngle = 0.5f * Mathf.Clamp(normalizedAngle, -1.0f, 1.0f) + 0.5f;

            normalizedAngles[i] = normalizedAngle;

            //draw the direction to the track point
            Debug.DrawLine(raycastOrigin.position, track.gameObject.transform.position + track.TrackPoints[upcomingTrackPoints[i]], Color.green, Time.fixedDeltaTime, false);
        }

        return normalizedAngles;
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

            Vector3 rayOrigin = raycastOrigin.transform.position;
            rayOrigin.y = other.GetController().raycastOrigin.transform.position.y;

            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, 30.0f, LayerMask.GetMask("Car"));

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.attachedRigidbody == rb)//the collider is part of the gaycaster car
                    continue;

                float prevDistance = Mathf.Sqrt(Mathf.Pow(raycastHit.x - raycastOrigin.transform.position.x, 2.0f) + Mathf.Pow(raycastHit.z - raycastOrigin.transform.position.z, 2.0f));
                if (raycastHit != Vector3.zero && hits[i].distance > prevDistance)
                    continue;

                raycastHit = hits[i].point;

                break;
            }
        }

        //draw ray
        if (raycastHit != Vector3.zero)
        {
            Debug.DrawLine(raycastOrigin.transform.position, raycastHit, new Color(0.0f, 1.0f, 1.0f), Time.fixedDeltaTime, false);
        }

        return raycastHit == Vector3.zero ?
            1.0f :
            Mathf.Clamp(
                Mathf.Sqrt(
                    Mathf.Pow(raycastHit.x - raycastOrigin.transform.position.x, 2.0f)
                    + Mathf.Pow(raycastHit.z - raycastOrigin.transform.position.z, 2.0f)
                    )
                / 30.0f,
                0.0f,
                1.0f
                );
    }
}
