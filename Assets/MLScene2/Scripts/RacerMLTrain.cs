using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class RacerMLTrain : RacerBase
{
    [SerializeField] private MLTrainController agent;
    [SerializeField] private IngameCarController carController;

    protected override void RacerFixedUpdate()
    {
        ApplyDownforce();
    }

    protected override void RacerUpdate()
    {
        ApplyBrakes();
        ApplyGas();
        ApplySteering();
    }

    protected override void RacerOnFinish()
    {
        
    }

    private void ApplyBrakes()
    {
        carController.BrakeInput = agent.BrakeInput;
    }

    private void ApplyGas()
    {
        carController.AccelInput = agent.AccelInput;
    }

    private void ApplySteering()
    {
        carController.SteerInput = agent.SteerInput;
    }

    private void ApplyDownforce()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float velocityMagnitude = Vector3.Dot(rb.velocity, transform.forward);
        rb.AddForce(-100.0f * Time.fixedDeltaTime * velocityMagnitude * transform.up, ForceMode.Impulse);
    }
}
