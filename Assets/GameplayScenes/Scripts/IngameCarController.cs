using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameCarController : MonoBehaviour
{
    [SerializeField] private BasedWheelCollider[] frontWheelColliders;
    [SerializeField] private BasedWheelCollider[] rearWheelColliders;

    [SerializeField] private Transform[] frontWheels;
    [SerializeField] private Transform[] rearWheels;

    [SerializeField] private float MAX_ACCELERATING_FORCE = 100.0f;
    [SerializeField] private float MAX_BRAKING_FORCE = 100.0f;

    [SerializeField] private float MAX_STEER_ANGLE = 15.0f;

    private float _accelInput = 0.0f;
    public float AccelInput
    {
        get { return _accelInput; }
        set { _accelInput = Mathf.Clamp(value, -1.0f, 1.0f); }
    }

    private float _brakeInput = 0.0f;
    public float BrakeInput
    {
        get { return _brakeInput; }
        set { _brakeInput = Mathf.Clamp(value, 0.0f, 1.0f); }
    }

    private float _steerInput = 0.0f;
    public float SteerInput
    {
        get { return _steerInput; }
        set { _steerInput = Mathf.Clamp(value, -1.0f, 1.0f); }
    }

    private void FixedUpdate()
    {
        ApplyGas();
        ApplyBrakes();
        ApplySteering();
        UpdateWheelPositions();
    }

    void ApplyGas()
    {
        foreach (BasedWheelCollider bwc in frontWheelColliders)
            bwc.AcceleratingForce = MAX_ACCELERATING_FORCE * AccelInput;
        foreach (BasedWheelCollider bwc in rearWheelColliders)
            bwc.AcceleratingForce = MAX_ACCELERATING_FORCE * AccelInput;
    }

    void ApplyBrakes()
    {
        foreach (BasedWheelCollider bwc in frontWheelColliders)
            bwc.BrakeForce = MAX_BRAKING_FORCE * BrakeInput;
        foreach (BasedWheelCollider bwc in rearWheelColliders)
            bwc.BrakeForce = MAX_BRAKING_FORCE * BrakeInput;
    }

    void ApplySteering()
    {
        foreach (BasedWheelCollider bwc in frontWheelColliders)
            bwc.SteerAngle = MAX_STEER_ANGLE * SteerInput;
    }

    void UpdateWheelPositions()
    {
        for(int i=0;i<frontWheelColliders.Length;i++)
        {
            Vector3 position;
            Quaternion rotation;

            frontWheelColliders[i].GetWorldPose(out position, out rotation);

            frontWheels[i].transform.position = position;
            frontWheels[i].transform.rotation= rotation;
        }

        for (int i = 0; i < rearWheelColliders.Length; i++)
        {
            Vector3 position;
            Quaternion rotation;

            rearWheelColliders[i].GetWorldPose(out position, out rotation);

            rearWheels[i].transform.position = position;
            rearWheels[i].transform.rotation = rotation;
        }
    }
}
