using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameCarController : MonoBehaviour
{
    [SerializeField] private IngameCarComponents carComponents;

    [SerializeField] private BasedWheelCollider[] frontWheelColliders;
    [SerializeField] private BasedWheelCollider[] rearWheelColliders;

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
    }

    private void Update()
    {
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
        Transform[] frontWheels=new Transform[] {carComponents.wheelFl, carComponents.wheelFr};
        Transform[] rearWheels=new Transform[] {carComponents.wheelRl, carComponents.wheelRr};

        for(int i=0;i<frontWheelColliders.Length;i++)
        {
            Vector3 position;
            Quaternion rotation;

            frontWheelColliders[i].GetWorldPose(out position, out rotation);

            //transform to local
            position = frontWheels[i].parent.InverseTransformPoint(position);
            rotation = Quaternion.Inverse(frontWheels[i].parent.rotation) * rotation;

            frontWheels[i].localPosition = Vector3.Lerp(frontWheels[i].localPosition, position, 0.5f);
            frontWheels[i].localRotation = Quaternion.Lerp(frontWheels[i].localRotation, rotation, 0.5f);
        }

        for (int i = 0; i < rearWheelColliders.Length; i++)
        {
            Vector3 position;
            Quaternion rotation;

            rearWheelColliders[i].GetWorldPose(out position, out rotation);

            //transform to local
            position = rearWheels[i].parent.InverseTransformPoint(position);
            rotation = Quaternion.Inverse(rearWheels[i].parent.rotation) * rotation;
            
            rearWheels[i].localPosition = Vector3.Lerp(rearWheels[i].localPosition, position, 0.5f);
            rearWheels[i].localRotation = Quaternion.Lerp(rearWheels[i].localRotation, rotation, 0.5f);
        }
    }
}
