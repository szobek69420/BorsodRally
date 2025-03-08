using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BasedWheelCollider : MonoBehaviour
{
    [SerializeField]
    private float _wheelRadius = 0.4f;
    public float WheelRadius
    {
        get { return _wheelRadius; }
        set { _wheelRadius = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
    }

    [SerializeField]
    private float _suspensionTarget = 0.3f;
    public float SuspensionTarget
    {
        get { return _suspensionTarget; }
        set { _suspensionTarget = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
    }

    [SerializeField]
    private float _maxSuspensionDistance = 0.1f;
    public float MaxSuspensionDistance
    {
        get { return _maxSuspensionDistance; }
        set { _maxSuspensionDistance = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
    }

    [SerializeField]
    private float _springForce = 30000.0f;
    public float SpringForce
    {
        get { return _springForce; }
        set { _springForce = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
    }

    [SerializeField]
    private float _dampingForce = 4000.0f;
    public float DampingForce
    {
        get { return _dampingForce; }
        set { _dampingForce = Mathf.Clamp(value, 0.0f, float.PositiveInfinity); }
    }

    private Rigidbody rb=null;

    private float currentSuspensionDistance = 0.0f;
    private bool wheelIsTouching = false;

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
    }

    // Update is called once per frame
    void Update()
    {
        DrawWheelPosition();
    }

    private void FixedUpdate()
    {
        GetCurrentSuspensionDistance();

        ApplySuspensionForce();
    }

    private void GetCurrentSuspensionDistance()
    {
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

    private void ApplySuspensionForce()
    {
        if(wheelIsTouching)
        {
            Vector3 pointVelocity = rb.GetPointVelocity(transform.position);

            float springForce = -currentSuspensionDistance * SpringForce;
            float dampingForce = -DampingForce * pointVelocity.y;

            rb.AddForceAtPosition(Time.fixedDeltaTime * Mathf.Max(0.0f,springForce + dampingForce) * Vector3.up, transform.position, ForceMode.Impulse);
        }
    }

    private void DrawWheelPosition()
    {
        /*DebugExtension.DrawCircle(
            transform.position + (SuspensionTarget + currentSuspensionDistance) * Vector3.down,
            transform.right,
            WheelRadius);*/

        Debug.DrawLine(transform.position, transform.position + (SuspensionTarget + currentSuspensionDistance) * Vector3.down, Color.green);
        Debug.DrawLine(
            transform.position + (SuspensionTarget + currentSuspensionDistance) * Vector3.down,
            transform.position + (SuspensionTarget + currentSuspensionDistance + WheelRadius) * Vector3.down,
            Color.red
            );
    }
}
