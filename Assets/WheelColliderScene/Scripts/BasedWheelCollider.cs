using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class BasedWheelCollider : MonoBehaviour
{
	[Tooltip("The radius of the wheel in meters [0.001, infinity)")]
	[SerializeField]
	private float _wheelRadius = 0.4f;
	public float WheelRadius
	{
		get { return _wheelRadius; }
		set { _wheelRadius = Mathf.Clamp(value, 0.001f, float.PositiveInfinity); }
	}


	[Tooltip("The target position of the suspension spring [0.0, infinity)")]
	[SerializeField]
	private float _suspensionTarget = 0.3f;
	public float SuspensionTarget
	{
		get { return _suspensionTarget; }
		set { _suspensionTarget = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
	}

	[Tooltip("The maximum suspension distance from the suspension target [0.0, SuspensionTarget)")]
	[SerializeField]
	private float _maxSuspensionDistance = 0.1f;
	public float MaxSuspensionDistance
	{
		get { return _maxSuspensionDistance; }
		set { _maxSuspensionDistance = Mathf.Clamp(value, 0.0f, SuspensionTarget); }
	}

	[Tooltip("The spring force of the shock absorbers [0;infinity)")]
	[SerializeField]
	private float _springForce = 30000.0f;
	public float SpringForce
	{
		get { return _springForce; }
		set { _springForce = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
	}

	[Tooltip("The damping force of the shock absorbers [0;infinity)")]
	[SerializeField]
	private float _dampingForce = 4000.0f;
	public float DampingForce
	{
		get { return _dampingForce; }
		set { _dampingForce = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
	}

	[Tooltip("The friction coefficient depending on the absolute value of slip")]
	public AnimationCurve FrictionCoefficient = AnimationCurve.Linear(0.0f, 0.9f, 1.0f, 0.5f);

	[Tooltip("The longitudinal stiffness of the tire, basically a multiplier for the friction coefficient. Influences acceleration. [0; infinity)")]
	[SerializeField]
	private float _longitudinalStiffness = 1.0f;
	public float LongitudinalStiffness
	{
		get { return _longitudinalStiffness; }
		set { _longitudinalStiffness = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
	}

	[Tooltip("The lateral stiffness of the tire, basically a multiplier for the friction coefficient, Influences steering. [0; infinity)")]
	[SerializeField]
	private float _lateralStiffness = 1.0f;
	public float LateralStiffness
	{
		get { return _lateralStiffness; }
		set { _lateralStiffness = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
	}

	public float Slip { get; private set; } = 0.0f; //<wheelVelocity; transform.right>

	public float AcceleratingForce { get; set; } = 0.0f;

	private float _brakeForce = 0.0f;
	public float BrakeForce
	{
		get { return _brakeForce; }
		set { _brakeForce = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
	}


	public float SteerAngle { get; set; } = 0.0f; //steer angle in degrees

	public float WheelRPM { get; private set; } = 0.0f;


	private Rigidbody rb=null;

	private Vector3 wheelPosition;
	private Vector3 wheelVelocity;
	private Vector3 wheelForward;
	private Vector3 wheelRight;
	private Vector3 wheelUp;

	private float currentSuspensionDistance = 0.0f;     //positive towards the extended state
	private bool wheelIsTouching = false;
	private float currentSuspensionForce = 0.0f;        //positive towards Vector3.up

	private float currentWheelRotation = 0.0f;      //radians
	private float currentWheelRPM = 0.0f;

	// Start is called before the first frame update
	void Start()
	{
		//find the rigidbody that is connected to the wheel
		Transform ancestor = transform;
		while(rb==null&& ancestor!=null)
		{
			Rigidbody temp = null;
			if (ancestor.gameObject.TryGetComponent<Rigidbody>(out temp))
				rb = temp;
			ancestor = ancestor.parent;
		}

		//init some values
		currentSuspensionDistance = SuspensionTarget;
		currentSuspensionForce = 0.0f;
		wheelIsTouching = false;
	}

	private void FixedUpdate()
	{
		wheelVelocity = rb.GetPointVelocity(transform.position);

		GetCurrentSuspensionDistance();

		CalculateWheelOrientation(false);
		CalculateSlip();

		ApplySuspensionForce();
		ApplyAcceleration();
		bool isSlipping=ApplyBrake();
		ApplySteering();

		CalculateWheelRotation(isSlipping);
	}

	public void OnDrawGizmos()
	{
		CalculateWheelOrientation(true);
		DrawWheelPosition();
	}

	private void CalculateWheelOrientation(bool calledByDrawGizmos)
	{
		Rigidbody rigidbody=null;

		if (calledByDrawGizmos)
		{
			Transform ancestor = transform;
			while (rigidbody == null && ancestor != null)
			{
				Rigidbody temp = null;
				if (ancestor.gameObject.TryGetComponent<Rigidbody>(out temp))
					rigidbody = temp;
				ancestor = ancestor.parent;
			}
		}
		else
			rigidbody = rb;


		if (rigidbody.gameObject.transform.up.y > 0.15f)//is the car upside down?
		{
			wheelPosition = transform.position + (SuspensionTarget + currentSuspensionDistance) * Vector3.down;

			float wheelRotY = Mathf.Atan2(transform.forward.x, transform.forward.z);
			wheelRotY += Mathf.Deg2Rad * SteerAngle;

			wheelForward = new Vector3(Mathf.Sin(wheelRotY), 0.0f, Mathf.Cos(wheelRotY));
			wheelRight = new Vector3(Mathf.Sin(wheelRotY+0.5f*Mathf.PI), 0.0f, Mathf.Cos(wheelRotY + 0.5f * Mathf.PI));
			wheelUp = Vector3.up;
		}
		else
		{
			wheelPosition = transform.position - (SuspensionTarget + currentSuspensionDistance) * transform.up;

			float steerAngleInRads = Mathf.Deg2Rad * SteerAngle;
			wheelForward = Mathf.Sin(steerAngleInRads) * transform.right + Mathf.Cos(steerAngleInRads) * transform.forward;
			wheelRight = Mathf.Sin(steerAngleInRads + 0.5f * Mathf.PI) * transform.right + Mathf.Cos(steerAngleInRads + 0.5f * Mathf.PI) * transform.forward;
			wheelUp = transform.up;
		}
	}

	private void GetCurrentSuspensionDistance()
	{
		//check if the car is upside down
		if(rb.gameObject.transform.up.y<0.15f)
		{
			wheelIsTouching = false;
			currentSuspensionDistance = MaxSuspensionDistance;
			return;
		}

		float tempSuspensionDistance = MaxSuspensionDistance;
		bool tempWheelIsTouching = false;

		//check if the wheel touches the ground
		//we need RaycastAll instead of Raycast, because it is possible, that the suspension goes through the chassis
		RaycastHit[] hits=Physics.RaycastAll(transform.position, Vector3.down, SuspensionTarget + MaxSuspensionDistance + WheelRadius);
		foreach(RaycastHit hit in hits)
		{
			if(rb!=hit.collider.attachedRigidbody)//hit found
			{
				tempWheelIsTouching = true;
				tempSuspensionDistance = hit.distance - WheelRadius - SuspensionTarget;
				break;
			}
		}

		wheelIsTouching = tempWheelIsTouching;
		currentSuspensionDistance = tempSuspensionDistance;
	}

	private void CalculateSlip()
	{
		//calculate slip
		if(wheelIsTouching)
		{
			Vector3 horizontalVelocityNormalized = Vector3.Normalize(new Vector3(wheelVelocity.x, 0.0f, wheelVelocity.z));
			Slip = Vector3.Dot(horizontalVelocityNormalized, wheelRight);
		}
		else
		{
			Slip = 0.0f;
		}
	}

	private void ApplySuspensionForce()
	{
		if (wheelIsTouching)
		{
			float springForce = -currentSuspensionDistance * SpringForce;
			float dampingForce = -DampingForce * wheelVelocity.y;

			currentSuspensionForce = Mathf.Max(0.0f, springForce + dampingForce);

			rb.AddForceAtPosition(Time.fixedDeltaTime * currentSuspensionForce * Vector3.up, transform.position, ForceMode.Impulse);
		}
		else
			currentSuspensionForce = 0.0f;
	}

	private void ApplyAcceleration()
	{
		float maxPossibleForce = LongitudinalStiffness*FrictionCoefficient.Evaluate(0.0f) * currentSuspensionForce;
		float appliedForce = Mathf.Clamp(AcceleratingForce, -maxPossibleForce, maxPossibleForce);
		Vector3 forceVector = appliedForce * wheelForward;
		rb.AddForceAtPosition(Time.fixedDeltaTime * forceVector, transform.position, ForceMode.Impulse);
	}

	//return value is true if the car is slipping
	private bool ApplyBrake()
	{
		bool isSlipping = false;

		float maxPossibleForce = LongitudinalStiffness * FrictionCoefficient.Evaluate(0.0f) * currentSuspensionForce;
		float appliedForce = Mathf.Clamp(BrakeForce, 0.0f, maxPossibleForce);

		if (BrakeForce > maxPossibleForce)
			isSlipping = true;

		//get the direction of the brake force
		//if the car is going forwards, the brake exterts force backwards
		if (0.0f < Vector3.Dot(wheelVelocity, transform.forward))
			appliedForce *= -1;

		Vector3 forceVector = appliedForce * wheelForward;

		rb.AddForceAtPosition(Time.fixedDeltaTime * forceVector, transform.position, ForceMode.Impulse);

		return isSlipping;
	}

	private void ApplySteering()
	{
		//apply lateral force
		float maxPossibleForce = LateralStiffness * FrictionCoefficient.Evaluate(Mathf.Abs(Slip)) * currentSuspensionForce;

		//steer
		if(Slip<0.0f)
			rb.AddForceAtPosition(Mathf.Pow(Mathf.Abs(Slip),0.5f)*Time.fixedDeltaTime * maxPossibleForce * wheelRight, transform.position, ForceMode.Impulse);
		else
			rb.AddForceAtPosition(-Mathf.Pow(Mathf.Abs(Slip), 0.5f) * Time.fixedDeltaTime * maxPossibleForce * wheelRight, transform.position, ForceMode.Impulse);
	}

	private void CalculateWheelRotation(bool isSlipping)
	{
		//calculate wheel rpm
		if (wheelIsTouching)
		{
			if (isSlipping)
				currentWheelRPM = 0.0f;
			else
				currentWheelRPM = 60.0f * Vector3.Dot(wheelForward, wheelVelocity) / (2.0f * Mathf.PI * WheelRadius);
		}

		//update wheel position
		currentWheelRotation += (Mathf.PI / 30.0f) * Time.fixedDeltaTime * currentWheelRPM;
	}

	private void DrawWheelPosition()
	{
		//draw solid suspension
		Gizmos.color = Color.green;
		Gizmos.DrawLine(transform.position, transform.position - SuspensionTarget * wheelUp);

		//draw the offset part
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(
			transform.position - SuspensionTarget * wheelUp,
			transform.position - (SuspensionTarget + currentSuspensionDistance) * wheelUp
			);


		//draw the wheel
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(wheelPosition, 0.02f);

		Gizmos.color = Color.red;
		for (int i = 0; i < 16; i++)
		{
			Vector3 startPosition = wheelPosition +
				WheelRadius * Mathf.Sin(0.125f * i * Mathf.PI) * wheelForward +
				WheelRadius * Mathf.Cos(0.125f * i * Mathf.PI) * wheelUp;
			Vector3 endPosition = wheelPosition +
				WheelRadius * Mathf.Sin(0.125f * (i + 1) * Mathf.PI) * wheelForward +
				WheelRadius * Mathf.Cos(0.125f * (i + 1) * Mathf.PI) * wheelUp;

			Gizmos.DrawLine(startPosition, endPosition);
		}
	}

	public void GetWorldPose(out Vector3 position, out Quaternion rotation)
	{
		//get position
		position = wheelPosition;

		//get rotation
		Vector3 rotatedWheelForward = Mathf.Cos(currentWheelRotation) * wheelForward - Mathf.Sin(currentWheelRotation) * wheelUp;
		Vector3 rotatedWheelUp = Mathf.Cos(currentWheelRotation) * wheelUp + Mathf.Sin(currentWheelRotation) * wheelForward;

		rotation = Quaternion.LookRotation(rotatedWheelForward, rotatedWheelUp);
	}
}
