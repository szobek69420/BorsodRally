using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RacerPlayer : RacerBase
{
    [SerializeField] private IngameCarController carController;
    [SerializeField] private Vector3 cameraOffset;

    private Vector3 previousCameraOffset = Vector3.zero;
    private Quaternion previousCameraRotation = Quaternion.identity;

    protected override void RacerUpdate()
    {
        UpdateCameraPosition();

        if (gameManager?.State != GameManagerBase.GameState.RACE)
            return;

        ApplyBrakes();
        ApplyGas();
        ApplySteering();
    }

    protected override void RacerFixedUpdate()
    {
        
    }
    
    protected override void RacerOnFinish()
    {
        gameManager?.EndRace();
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
        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 velocity = rb == null ? Vector3.zero : rb.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0.0f, velocity.z);
        Vector3 offset;
        Quaternion rotation;

        if (horizontalVelocity.magnitude < 1.0f)
        {
            offset =
                cameraOffset.z * transform.forward +
                cameraOffset.x * transform.right +
                cameraOffset.y * Vector3.up;
            rotation = Quaternion.LookRotation(transform.up - offset);
        }
        else
        {
            Vector3 forward = horizontalVelocity.normalized;
            Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.up, forward));

            float speedCoeff = 1.0f + 0.01f * horizontalVelocity.magnitude;
            offset =
               cameraOffset.z * forward +
               cameraOffset.x * right +
               cameraOffset.y * Vector3.up;
            offset = new Vector3(speedCoeff * offset.x, offset.y, speedCoeff * offset.z);
            rotation = Quaternion.LookRotation(transform.up - offset);
        }

        previousCameraOffset = Vector3.Lerp(previousCameraOffset, offset, 0.2f);
        previousCameraRotation = Quaternion.Lerp(previousCameraRotation, rotation, 0.2f);

        Camera.main.transform.position = transform.position + previousCameraOffset;
        Camera.main.transform.rotation = previousCameraRotation;
    }
}
