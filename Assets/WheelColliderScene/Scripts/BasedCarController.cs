using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasedCarController : MonoBehaviour
{
    [SerializeField] private BasedWheelCollider[] frontWheelColliders;
    [SerializeField] private BasedWheelCollider[] rearWheelColliders;

    [SerializeField] private float MAX_ACCELERATING_FORCE = 100.0f;
    [SerializeField] private float MAX_BRAKING_FORCE = 100.0f;

    [SerializeField] private float MAX_STEER_ANGLE = 15.0f;

    [SerializeField] private Vector3 cameraOffset;

    [SerializeField] private TMPro.TMP_Text speedo;

    private void FixedUpdate()
    {
        //accelerate
        float accelForce = Input.GetAxis("Vertical") * MAX_ACCELERATING_FORCE;
        foreach (BasedWheelCollider bwc in frontWheelColliders)
            bwc.AcceleratingForce = accelForce;
        foreach (BasedWheelCollider bwc in rearWheelColliders)
            bwc.AcceleratingForce = accelForce;


        //also accelerate
        float brakeForce = Input.GetKey(KeyCode.Space) ? MAX_BRAKING_FORCE : 0.0f;
        foreach (BasedWheelCollider bwc in frontWheelColliders)
            bwc.BrakeForce = brakeForce;
        foreach (BasedWheelCollider bwc in rearWheelColliders)
            bwc.BrakeForce = brakeForce;

        //accelerate yet again
        float steerAngle = Input.GetAxisRaw("Horizontal") * MAX_STEER_ANGLE;
        foreach (BasedWheelCollider bwc in frontWheelColliders)
            bwc.SteerAngle = steerAngle;


        Camera.main.transform.position = transform.position + cameraOffset.x * transform.right + cameraOffset.y * transform.up + cameraOffset.z * transform.forward;
        Camera.main.transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    
        speedo.text=((int)(3.6f*GetComponent<Rigidbody>().velocity.magnitude)).ToString();
    }
}
