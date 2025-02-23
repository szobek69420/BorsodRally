using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullGripCar : Car
{
    [SerializeField] private float wheelBase = 3.0f;  //necessary for calculating the turning rate
    [SerializeField] private float brakeForce = 10000.0f;
    [SerializeField] private float accelerationForce = 2500.0f;
    [SerializeField] private float MAX_VELOCITY = 70.0f;

    [SerializeField] private Transform[] frontWheels;

    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Steer();
        Accelerate();

        UpdateWheelPosition();
    }

    private void Steer()
    {
        if (Mathf.Abs(SteerAngle) < 0.00001f)//kein turning
            return;

        float forwardVelocity = Vector3.Dot(transform.forward, rb.velocity);
        float turningRadius = wheelBase / Mathf.Sin(Mathf.Deg2Rad * MAX_STEER_ANGLE*SteerAngle);
        float turningRate = Mathf.Rad2Deg*forwardVelocity / turningRadius;

        transform.localRotation = Quaternion.Euler(
            0,
            transform.localRotation.eulerAngles.y + Time.fixedDeltaTime * turningRate,
            0);

        rb.velocity = forwardVelocity * transform.forward;
    }

    private void Accelerate()
    {
        //throttle
        rb.AddRelativeForce(
            Time.fixedDeltaTime * Throttle * accelerationForce * Vector3.forward, 
            ForceMode.Impulse
            );

        if(Vector3.Dot(transform.forward, rb.velocity) > MAX_VELOCITY)
            rb.velocity*=MAX_VELOCITY/rb.velocity.magnitude;
        
        //brake
        if(Vector3.Dot(transform.forward, rb.velocity)>0.0f)
            rb.AddRelativeForce(
                -Time.fixedDeltaTime * Brake * brakeForce * Vector3.forward,
                ForceMode.Impulse
                );
        else
        {
            rb.AddRelativeForce(
                0.25f*Time.fixedDeltaTime * Throttle * accelerationForce * Vector3.forward,
                ForceMode.Impulse
                );
        }
    }

    private void UpdateWheelPosition()
    {
        foreach(Transform wheel in frontWheels)
        {
            wheel.localRotation = Quaternion.Euler(0,MAX_STEER_ANGLE * SteerAngle, 0);
        }
    }
}
