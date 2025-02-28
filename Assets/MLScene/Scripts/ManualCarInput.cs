using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualCarInput : MonoBehaviour
{
    [SerializeField] private Car car;

    private void FixedUpdate()
    {
        car.SteerAngle = Input.GetAxisRaw("Horizontal");

        car.Throttle = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.Space))
            car.Brake = 1.0f;
        else
            car.Brake = 0.0f;
    }
}
