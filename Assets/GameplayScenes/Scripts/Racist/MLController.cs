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
		new Vector3(Mathf.Sin(0.5f*Mathf.PI), 0.0f, Mathf.Cos(0.5f*Mathf.PI))
	};
	private static float RAYCAST_MAX_DISTANCE = 150.0f;

	private GameManagerBase gm = null;
	private RacetrackGenerator track = null;
	[SerializeField] private Rigidbody rb;
	[SerializeField] private Transform raycastOrigin;

	public float SteerInput { get; private set; } = 0;
	public float AccelInput{ get; private set; } = 0;
	public float BrakeInput { get; private set; } = 0;

	private void Start()
	{

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
		float[] distances = Raycast();
		foreach(float distance in distances)
			sensor.AddObservation(distance/RAYCAST_MAX_DISTANCE);

		//alignment to the next checkpoint
		float alignment = 0.0f;
		if (!isStationary)
		{
			int nextCheckpointIndex = track.GetNearestTrackPointIndex(transform.position)+1;
			if(nextCheckpointIndex>=track.TrackPoints.Count)
				nextCheckpointIndex=track.TrackPoints.Count-1;
			Vector3 nextCheckpointPosition=track.TrackPoints[nextCheckpointIndex];

			alignment = Mathf.Atan2(
				Vector3.Dot(Vector3.Normalize(nextCheckpointPosition - transform.position), transform.right),
				Vector3.Dot(Vector3.Normalize(nextCheckpointPosition-transform.position), transform.forward)
				);
		}
		alignment /= Mathf.PI;
		alignment = 0.5f * alignment + 0.5f;
		alignment = Mathf.Clamp(alignment, 0.0f, 1.0f);
		sensor.AddObservation(alignment / Mathf.PI);

		//velocity
		sensor.AddObservation(0.002f * speed);

		//tilt
		sensor.AddObservation(CalculateTilt());

		//add reward for going in the right direction at high speed
		AddReward(0.01f * (0.5f - Mathf.Abs(alignment - 0.5f)));

		//punish slow driving
		if (speed < 30.0f)
			AddReward(0.02f * (speed - 30.0f));
	}

	public override void OnActionReceived(ActionBuffers actions)
	{
		SteerInput = actions.ContinuousActions[0];
		AccelInput = actions.ContinuousActions[1];
		BrakeInput = 0.2f * actions.DiscreteActions[0];

		AddReward(-BrakeInput);
	}

	private float[] Raycast()
	{
		float[] distances = new float[RAYCAST_DIRECTIONS.Length];

        int mask = LayerMask.GetMask("Track");

        Vector3 forward = transform.forward;
        if (Mathf.Pow(rb.velocity.x, 2.0f) + Mathf.Pow(rb.velocity.z, 2.0f) > 1.0f)
            forward = rb.velocity.normalized;
        Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.up, forward));

        for (int i=0;i<RAYCAST_DIRECTIONS.Length;i++)
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
}
