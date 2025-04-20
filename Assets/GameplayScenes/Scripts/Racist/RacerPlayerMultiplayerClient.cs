using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//it is the racer that is piloted by the client
public class RacerPlayerMultiplayerClient : RacerBase
{
	[SerializeField] private Vector3 cameraOffset;

	private Vector3 cameraVelocity=Vector3.zero;
	public Vector3 Velocity { get; set; }=Vector3.zero;

	protected override void RacerUpdate()
	{
		
	}

	protected override void RacerFixedUpdate()
	{
        (gameManager as GameManagerMultiplayer).UpdateClientInput(
            new CarInput(
                this.GetComponent<RacerId>().id,
                ApplyGas(),
                ApplyBrakes(),
                ApplySteering()
                )
            );

		cameraVelocity = Vector3.Lerp(cameraVelocity, Velocity, 0.3f);

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
		Vector3 velocity = cameraVelocity;
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
