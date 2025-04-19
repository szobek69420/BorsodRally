using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//it is the racer that is piloted by the client
public class RacerPlayerMultiplayerClient : RacerBase
{
    [SerializeField] private Vector3 cameraOffset;

    public int PlayerId { get; set; }=69;
    public Vector3 velocity { get; set; } = Vector3.zero;

    protected override void RacerUpdate()
    {
        (gameManager as GameManagerMultiplayer).UpdateClientInput(
            new CarInput(
                PlayerId,
                ApplyGas(),
                ApplyBrakes(),
                ApplySteering()
                )
            );
    }

    protected override void RacerFixedUpdate()
    {
        UpdateCameraPosition();
    }

    protected override void RacerOnFinish()
    {
        throw new System.NotImplementedException();
    }

    private float ApplyBrakes()
    {
        return Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    }

    private float ApplyGas()
    {
        return Input.GetAxisRaw("Vertical");
    }

    private float ApplySteering()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    private void UpdateCameraPosition()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (velocity.magnitude < 1.0f)
        {
            position =
                transform.position +
                cameraOffset.z * transform.forward +
                cameraOffset.x * transform.right +
                cameraOffset.y * Vector3.up;
            rotation = Quaternion.LookRotation(transform.position + transform.up - position);
        }
        else
        {
            Vector3 forward = velocity;
            forward.y = 0.0f;
            forward = Vector3.Normalize(forward);
            Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.up, forward));

            position =
               transform.position +
               cameraOffset.z * forward +
               cameraOffset.x * right +
               cameraOffset.y * Vector3.up;
            rotation = Quaternion.LookRotation(transform.position + transform.up - position);
        }


        Camera.main.transform.position = Vector3.Lerp(
                    Camera.main.transform.position,
                    position,
                    0.2f
                    );

        Camera.main.transform.rotation = Quaternion.Lerp(
            Camera.main.transform.rotation,
            rotation,
            0.2f
            );
    }
}
