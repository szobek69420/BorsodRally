using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealisticCar : Car
{
    [SerializeField] private float maxBrakeTorque = 10000.0f;
    [SerializeField] private float maxMotorTorque = 2500.0f;
    [SerializeField] private float MAX_VELOCITY = 70.0f;


    [SerializeField] private BasedWheelCollider[] frontWheelColliders;
    [SerializeField] private BasedWheelCollider[] rearWheelColliders;

    [SerializeField] private Transform[] frontWheels;
    [SerializeField] private Transform[] rearWheels;


    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private Transform centerOfMass;

    [SerializeField] private MeshRenderer chassisRenderer;
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material brakingMaterial;

    private Rigidbody rb;

    public const float RAYCAST_MAX_DISTANCE = 100.0f;
    private Vector2[] raycastDirections = new Vector2[5]
    {
        new(Mathf.Sin(-0.5f*Mathf.PI), Mathf.Cos(-0.5f*Mathf.PI)),
        new(Mathf.Sin(-0.15f*Mathf.PI), Mathf.Cos(-0.15f*Mathf.PI)),
        new(Mathf.Sin(0), Mathf.Cos(0)),
        new(Mathf.Sin(0.15f*Mathf.PI), Mathf.Cos(0.15f*Mathf.PI)),
        new(Mathf.Sin(0.5f*Mathf.PI), Mathf.Cos(0.5f*Mathf.PI))
    };
    public float[] distanceFromWall = new float[5];

    public float tiltNormalized = 0.0f;//the angle between the velocity vector and the forward direction

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;
    }

    // Update is called once per frame
    private void Update()
    {
        SetMaterial();
        VisualizeGaycast();
    }

    private void FixedUpdate()
    {
        Steer();
        Accelerate();

        UpdateWheelPosition();

        Gaycast();
        CalculateTilt();
    }

    private void Steer()
    {
        foreach (BasedWheelCollider wc in frontWheelColliders)
            wc.SteerAngle=MAX_STEER_ANGLE* SteerAngle;
    }

    private void Accelerate()
    {
        //throttle
        float motorTorque = Throttle * maxMotorTorque;
        if(motorTorque > 0&&rb.velocity.magnitude>MAX_VELOCITY)
            motorTorque = 0;
        foreach (BasedWheelCollider wc in frontWheelColliders)
            wc.AcceleratingForce = motorTorque;
        foreach (BasedWheelCollider wc in rearWheelColliders)
            wc.AcceleratingForce = motorTorque;

        //brake
        foreach (BasedWheelCollider wc in frontWheelColliders)
            wc.BrakeForce = Brake * maxBrakeTorque;
        foreach (BasedWheelCollider wc in rearWheelColliders)
            wc.BrakeForce = Brake * maxBrakeTorque;
    }

    private void UpdateWheelPosition()
    {
        Vector3 wheelPosition;
        Quaternion wheelRotation;

        for (int i = 0; i < frontWheelColliders.Length; i++)
        {
            frontWheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            frontWheels[i].position = wheelPosition;
            frontWheels[i].rotation = wheelRotation;
        }

        for (int i = 0; i < rearWheelColliders.Length; i++)
        {
            rearWheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            rearWheels[i].position = wheelPosition;
            rearWheels[i].rotation = wheelRotation;
        }
    }

    void Gaycast()
    {
        int mask = LayerMask.GetMask("Track");

        Vector3 forward = transform.forward;
        if (Mathf.Pow(rb.velocity.x, 2.0f) + Mathf.Pow(rb.velocity.z, 2.0f) > 1.0f)
            forward = rb.velocity.normalized;
        Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.up, forward));

        for (int i = 0; i < raycastDirections.Length; i++)
        {
            Vector3 raycastDirection =
                raycastDirections[i].x * right +
                raycastDirections[i].y * forward;
            RaycastHit hit;
            if (Physics.Raycast(
                raycastOrigin.position,
                raycastDirection,
                out hit,
                RAYCAST_MAX_DISTANCE,
                mask))
            {
                distanceFromWall[i] = hit.distance;
            }
            else
            {
                distanceFromWall[i] = RAYCAST_MAX_DISTANCE;
            }
        }
    }

    //calculates the normalized tilt
    void CalculateTilt()
    {
        float tempTiltNormalized = Mathf.Acos(Vector3.Dot(transform.forward, rb.velocity.normalized));
        if (Vector3.Dot(rb.velocity, transform.right) < 0.0f)
            tempTiltNormalized *= -1;
        tempTiltNormalized /= 0.5f * Mathf.PI;
        tempTiltNormalized = Mathf.Clamp(tempTiltNormalized, - 1.0f, 1.0f);

        tiltNormalized = 0.5f*tempTiltNormalized+0.5f;
    }

    void VisualizeGaycast()
    {
        Vector3 forward = transform.forward;
        if (Mathf.Pow(rb.velocity.x, 2.0f) + Mathf.Pow(rb.velocity.z, 2.0f) > 1.0f)
            forward = rb.velocity.normalized;
        Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.up, forward));

        for (int i = 0; i < raycastDirections.Length; i++)
        {
            Vector3 raycastDirection =
                raycastDirections[i].x * right +
                raycastDirections[i].y * forward;

            Debug.DrawLine(
                raycastOrigin.position,
                raycastOrigin.position + distanceFromWall[i] * raycastDirection,
                new Color(0.0f, 1.0f, 0.5f),
                0);

            DebugExtension.DebugPoint(
                raycastOrigin.position + distanceFromWall[i] * raycastDirection,
                5,
                0,
                true
                );
        }

        Debug.DrawLine(transform.position, GetComponentInParent<TrackManager>().NextCheckpointPosition(transform.position), Color.red, 0);
    }

    private void SetMaterial()
    {
        if (chassisRenderer == null)
            return;

        if (Brake > 0.001f)
            chassisRenderer.material = brakingMaterial;
        else
            chassisRenderer.material = normalMaterial;
    }
}
