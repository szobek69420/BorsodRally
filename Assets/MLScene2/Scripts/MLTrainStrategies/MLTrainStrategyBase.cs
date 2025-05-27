using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public abstract class MLTrainStrategyBase : MonoBehaviour
{
    protected MLTrainController controller;

    protected static Vector3[] RAYCAST_DIRECTIONS = new Vector3[]
    {
        new Vector3(Mathf.Sin(-0.5f*Mathf.PI), 0.0f, Mathf.Cos(-0.5f*Mathf.PI)),
        new Vector3(Mathf.Sin(-0.25f*Mathf.PI), 0.0f, Mathf.Cos(-0.25f*Mathf.PI)),
        new Vector3(Mathf.Sin(0.0f*Mathf.PI), 0.0f, Mathf.Cos(0.0f*Mathf.PI)),
        new Vector3(Mathf.Sin(0.25f*Mathf.PI), 0.0f, Mathf.Cos(0.25f*Mathf.PI)),
        new Vector3(Mathf.Sin(0.5f*Mathf.PI), 0.0f, Mathf.Cos(0.5f*Mathf.PI)),
        new Vector3(Mathf.Sin(Mathf.PI), 0.0f, Mathf.Cos(Mathf.PI)) //backwards should be the last one
    };
    protected static float RAYCAST_MAX_DISTANCE = 150.0f;
    protected static float RAYCAST_MAX_DISTANCE_BACKWARDS = 15.0f;//short so that most of the time it doesn't interfere

    protected float lastProgress = 0.0f;
    protected float startTime = 0.0f;
    protected float[] distances = null;
    protected float[] normalizedAngles = null;
    protected float tilt = 0.0f;

    public void SetController(MLTrainController controller)
    {
        this.controller = controller;
    }

    public MLTrainController GetController()
    {
        return this.controller;
    }

    //fixed update is used for obtaining sensor values
    public void FixedUpdate() {
        distances = Raycast();
        normalizedAngles = CalculateNormalizedAngles();
        tilt= CalculateTilt();

        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate() { }
    public virtual void OnOnTriggerEnter(Collider other) { }
    public virtual void OnEpisodeBegin() { }
    public abstract void CollectObservations(VectorSensor sensor);
    public virtual void Dieded(float reward)
    {
        controller.AddReward(reward);
        controller.EndEpisode();
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
                    controller.raycastOrigin.position,
                    raycastDirection,
                    out hit,
                    RAYCAST_MAX_DISTANCE,
                    mask)
                    )
                    distances[i] = hit.distance/RAYCAST_MAX_DISTANCE;
                else
                    distances[i] = 1.0f;

                //draw ray
                Debug.DrawLine(controller.raycastOrigin.position, controller.raycastOrigin.position + distances[i] * RAYCAST_MAX_DISTANCE*raycastDirection, Color.red, Time.fixedDeltaTime, false);
            }
            else//backwards direction
            {
                RaycastHit hit;
                if (Physics.Raycast(
                    controller.raycastOrigin.position,
                    raycastDirection,
                    out hit,
                    RAYCAST_MAX_DISTANCE_BACKWARDS,
                    mask)
                    )
                    distances[i] = hit.distance/RAYCAST_MAX_DISTANCE_BACKWARDS;
                else
                    distances[i] = 1.0f;

                //draw ray
                Debug.DrawLine(controller.raycastOrigin.position, controller.raycastOrigin.position + distances[i] * RAYCAST_MAX_DISTANCE_BACKWARDS * raycastDirection, Color.red, Time.fixedDeltaTime, false);
            }
        }


        return distances;
    }

    //calculates the normalized tilt which is the angle between the velocity vector and the forward direction squeezed into [0;1]
    float CalculateTilt()
    {
        float tiltNormalized = 0.0f;

        if (controller.rb.velocity.magnitude < 0.01f)
            return 0.5f;

        tiltNormalized = Mathf.Acos(Vector3.Dot(transform.forward, controller.rb.velocity.normalized));
        if (Vector3.Dot(controller.rb.velocity, transform.right) < 0.0f)
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
        if (controller.rb.velocity.sqrMagnitude < 2.0f)
            velocityNormalized = transform.forward;
        else
            velocityNormalized = controller.rb.velocity.normalized;
        Vector3 horizontalVelocityNormalized = new Vector3(velocityNormalized.x, 0.0f, velocityNormalized.z).normalized;

        int currentTrackPoint = controller.track.GetNearestTrackPointIndex(controller.rb.position - controller.track.transform.position);

        int[] upcomingTrackPoints = new int[2];
        upcomingTrackPoints[0] = Mathf.Clamp(currentTrackPoint + 30, currentTrackPoint, controller.track.TrackPoints.Count - 1);
        upcomingTrackPoints[1] = Mathf.Clamp(currentTrackPoint + 80, currentTrackPoint, controller.track.TrackPoints.Count - 1);

        float[] normalizedAngles = new float[2];

        for (int i = 0; i < upcomingTrackPoints.Length; i++)
        {
            Vector3 trackPointDirection = controller.track.TrackPoints[upcomingTrackPoints[i]] - controller.track.TrackPoints[currentTrackPoint];
            trackPointDirection.y = 0.0f;
            trackPointDirection = Vector3.Normalize(trackPointDirection);

            float normalizedAngle = Mathf.Acos(Vector3.Dot(trackPointDirection, horizontalVelocityNormalized));
            if (0.0f > Vector3.Dot(Vector3.Cross(Vector3.up, trackPointDirection), horizontalVelocityNormalized))
                normalizedAngle *= -1;

            normalizedAngle /= 0.5f * Mathf.PI;
            normalizedAngle = 0.5f * Mathf.Clamp(normalizedAngle, -1.0f, 1.0f) + 0.5f;

            normalizedAngles[i] = normalizedAngle;

            //draw the direction to the track point
            Debug.DrawLine(controller.raycastOrigin.position, controller.track.gameObject.transform.position + controller.track.TrackPoints[upcomingTrackPoints[i]], Color.green, Time.fixedDeltaTime, false);
        }

        return normalizedAngles;
    }
}
