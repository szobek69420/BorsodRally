using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCarController : MonoBehaviour
{
    public float MAX_ACCELERATION_FORCE = 2000.0f;
    public float MAX_BRAKE_FORCE = 10000.0f;
    public float MAX_STEER_ANGLE = 20.0f;

    private float _accelerationInput = 0.0f;
    public float AccelerationInput
    {
        get { return _accelerationInput; }
        set { _accelerationInput = Mathf.Clamp(value, -1.0f, 1.0f); }
    }

    private float _brakeInput = 0.0f;
    public float BrakeInput
    {
        get { return _brakeInput; }
        set { _brakeInput = Mathf.Clamp(value, 0.0f, 1.0f); }
    }

    private float _steeringInput = 0.0f;
    public float SteeringInput
    {
        get { return _steeringInput; }
        set { _steeringInput = Mathf.Clamp(value, -1.0f, 1.0f); }
    }


    [SerializeField] private BasedWheelCollider wcFl;
    [SerializeField] private BasedWheelCollider wcFr;
    [SerializeField] private BasedWheelCollider wcRl;
    [SerializeField] private BasedWheelCollider wcRr;

    private BasedWheelCollider[] frontWCs;
    private BasedWheelCollider[] rearWCs;

    private Transform[] frontWheels;
    private Transform[] rearWheels;

    // Start is called before the first frame update
    void Start()
    {
        NetworkCarComponents ncc = GetComponent<NetworkCarComponents>();

        frontWCs = new BasedWheelCollider[] { wcFl, wcFr };
        rearWCs = new BasedWheelCollider[] { wcRl, wcRr };

        frontWheels=new Transform[] {ncc.wheelFl, ncc.wheelFr };
        rearWheels=new Transform[] {ncc.wheelRl, ncc.wheelRr };
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var wc in frontWCs)
        {
            wc.AcceleratingForce = MAX_ACCELERATION_FORCE * AccelerationInput;
            wc.BrakeForce=MAX_BRAKE_FORCE * BrakeInput;
            wc.SteerAngle = MAX_STEER_ANGLE * SteeringInput;
        }
        foreach (var wc in rearWCs)
        {
            wc.AcceleratingForce = MAX_ACCELERATION_FORCE * AccelerationInput;
            wc.BrakeForce = MAX_BRAKE_FORCE * BrakeInput;
        }

        for(int i=0;i<frontWheels.Length;i++)
        {
            Vector3 wheelPos;
            Quaternion wheelRot;
            frontWCs[i].GetWorldPose(out wheelPos, out wheelRot);

            frontWheels[i].transform.position = wheelPos;
            frontWheels[i].transform.rotation = wheelRot;
        }
        for (int i = 0; i < rearWheels.Length; i++)
        {
            Vector3 wheelPos;
            Quaternion wheelRot;
            rearWCs[i].GetWorldPose(out wheelPos, out wheelRot);

            rearWheels[i].transform.position = wheelPos;
            rearWheels[i].transform.rotation = wheelRot;
        }
    }
}
