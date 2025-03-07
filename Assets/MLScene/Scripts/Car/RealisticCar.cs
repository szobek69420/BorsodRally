using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealisticCar : Car
{
    [SerializeField] private float maxBrakeTorque = 10000.0f;
    [SerializeField] private float maxMotorTorque = 2500.0f;
    [SerializeField] private float MAX_VELOCITY = 70.0f;


    [SerializeField] private WheelCollider[] frontWheelColliders;
    [SerializeField] private WheelCollider[] rearWheelColliders;

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
    }

    private void Steer()
    {
        foreach (WheelCollider wc in frontWheelColliders)
            wc.steerAngle=MAX_STEER_ANGLE* SteerAngle;
    }

    private void Accelerate()
    {
        //throttle
        float motorTorque = Throttle * maxMotorTorque;
        if(motorTorque > 0&&rb.velocity.magnitude>MAX_VELOCITY)
            motorTorque = 0;
        foreach (WheelCollider wc in frontWheelColliders)
            wc.motorTorque = motorTorque;
        foreach (WheelCollider wc in rearWheelColliders)
            wc.motorTorque = motorTorque;

        //brake
        foreach (WheelCollider wc in frontWheelColliders)
            wc.brakeTorque = Brake * maxBrakeTorque;
        foreach (WheelCollider wc in rearWheelColliders)
            wc.brakeTorque = Brake * maxBrakeTorque;
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

        for (int i = 0; i < raycastDirections.Length; i++)
        {
            Vector3 raycastDirection =
                raycastDirections[i].x * transform.right +
                raycastDirections[i].y * transform.forward;
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

    void VisualizeGaycast()
    {
        for (int i = 0; i < raycastDirections.Length; i++)
        {
            Vector3 raycastDirection =
                raycastDirections[i].x * transform.right +
                raycastDirections[i].y * transform.forward;

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
