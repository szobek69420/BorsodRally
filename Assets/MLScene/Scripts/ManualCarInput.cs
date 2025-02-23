using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualCarInput : MonoBehaviour
{
    [SerializeField] private Car car;

    private void FixedUpdate()
    {
        float steerAngle = 0.0f;
        if (Input.GetKey(KeyCode.A))
            steerAngle -= 1.0f;
        if (Input.GetKey(KeyCode.D))
            steerAngle += 1.0f;

        car.SteerAngle= steerAngle;


        if (Input.GetKey(KeyCode.W))
            car.Throttle = 1.0f;
        else
            car.Throttle = 0.0f;


        if (Input.GetKey(KeyCode.Space))
            car.Brake = 1.0f;
        else
            car.Brake = 0.0f;
    }
}
