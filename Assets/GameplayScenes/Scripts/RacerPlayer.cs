using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RacerPlayer : RacerBase
{
    [SerializeField] private Vector3 cameraOffset;

    protected override void RacerUpdate()
    {
        ApplyBrakes();
        ApplyGas();
        ApplySteering();
        UpdateCameraPosition();
    }

    protected override void RacerFixedUpdate()
    {
        UpdateCameraPosition();
    }

    private void ApplyBrakes()
    {
        carController.BrakeInput = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    }

    private void ApplyGas()
    {
        carController.AccelInput = Input.GetAxisRaw("Vertical");
    }

    private void ApplySteering()
    {
        carController.SteerInput = Input.GetAxisRaw("Horizontal");
    }

    private void UpdateCameraPosition()
    {
        Rigidbody rb= GetComponent<Rigidbody>();

        Vector3 position=Vector3.zero;
        Quaternion rotation=Quaternion.identity;

        if (rb==null||rb.velocity.magnitude<1.0f)
        {
            position =
                transform.position +
                cameraOffset.z * transform.forward +
                cameraOffset.x * transform.right +
                cameraOffset.y * Vector3.up;
            rotation = Quaternion.LookRotation(transform.position - position);
        }
        else
        {
            Vector3 forward = rb.velocity;
            forward.y = 0.0f;
            forward=Vector3.Normalize(forward);
            Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.up, forward));

            position =
               transform.position +
               cameraOffset.z * forward +
               cameraOffset.x * right +
               cameraOffset.y * Vector3.up;
            rotation = Quaternion.LookRotation(transform.position - position);
        }


        Camera.main.transform.position = Vector3.Lerp(
                    Camera.main.transform.position,
                    position,
                    0.1f
                    );

        Camera.main.transform.rotation = Quaternion.Lerp(
            Camera.main.transform.rotation,
            rotation,
            0.1f
            );
    }
}
