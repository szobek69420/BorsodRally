using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Car : MonoBehaviour
{
    public float MAX_STEER_ANGLE = 20.0f;

    //SteerAngle is a float in [-1;1] and represents the steer angle between -MAX_STEER_ANGLE and MAX_STEER_ANGLE
    private float steerAngle = 0.0f;
    public float SteerAngle
    {
        get { return steerAngle; }
        set
        {
            if (value < -1 || value > 1)
                throw new System.Exception("Car::SteerAngle must be a value between -1 and 1");
            steerAngle = value;
        }
    }

    //Throttle is a value in [0;1]
    private float throttle = 0.0f;
    public float Throttle
    {
        get { return throttle; }
        set
        {
            if (value < 0 || value > 1)
                throw new System.Exception("Car::Throttle must be a value between 0 and 1");
            throttle = value;
        }
    }

    //Brake is a value in [0;1]
    private float brake = 0.0f;
    public float Brake
    {
        get { return brake; }
        set
        {
            if (value < 0 || value > 1)
                throw new System.Exception("Car::Brake must be a value between 0 and 1");
            brake = value;
        }
    }
}
