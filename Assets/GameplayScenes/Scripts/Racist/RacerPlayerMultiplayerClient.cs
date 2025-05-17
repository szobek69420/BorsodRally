using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//it is the racer that is piloted by the client
public class RacerPlayerMultiplayerClient : RacerBase
{
	[SerializeField] private Vector3 cameraOffset;
	public Vector3 Velocity { get; set; }=Vector3.zero;

	private Vector3 previousCameraOffset=Vector3.zero;
	private Quaternion previousCameraRotation=Quaternion.identity;

	protected override void RacerUpdate()
	{
        if (gameManager?.State != GameManagerBase.GameState.RACE)
            return;

        (gameManager as GameManagerMultiplayer).UpdateClientInput(
            new CarInput(
                this.GetComponent<RacerId>().id,
                ApplyGas(),
                ApplyBrakes(),
                ApplySteering()
                )
            );

    }

	protected override void RacerFixedUpdate()
	{
		//update orientation and velocity
		NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        carComponents.InterpolateOrientation(networkManager.ServerTime.TimeAsFloat, 0.05f);
        Velocity = carComponents.InterpolateVelocity(networkManager.ServerTime.TimeAsFloat, 0.05f);
        
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
		Vector3 velocity = Velocity;
		Vector3 horizontalVelocity = new Vector3(velocity.x, 0.0f, velocity.z);
		Vector3 offset;
		Quaternion rotation;

		if (horizontalVelocity.magnitude < 1.0f)
		{
			offset =
				cameraOffset.z * transform.forward +
				cameraOffset.x * transform.right +
				cameraOffset.y * Vector3.up;
			rotation = Quaternion.LookRotation(transform.up-offset);
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
			offset=new Vector3(speedCoeff*offset.x, offset.y, speedCoeff*offset.z);
			rotation = Quaternion.LookRotation(transform.up - offset);
		}

		previousCameraOffset = Vector3.Lerp(previousCameraOffset, offset, 0.2f);
		previousCameraRotation = Quaternion.Lerp(previousCameraRotation, rotation, 0.2f);

		Camera.main.transform.position = transform.position+previousCameraOffset;
		Camera.main.transform.rotation = previousCameraRotation;
	}
}
