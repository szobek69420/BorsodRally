using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerML : RacerBase
{
    [SerializeField] private MLController agent;
    [SerializeField] private IngameCarController carController;

    protected override void RacerUpdate()
    {
        if (gameManager?.State != GameManagerBase.GameState.RACE)
            return;

        ApplyBrakes();
        ApplyGas();
        ApplySteering();
    }

    protected override void RacerFixedUpdate()
    {
        if (gameManager?.State != GameManagerBase.GameState.RACE)
            return;

        ApplyDownforce();
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
        float forwardVelocity = Mathf.Clamp(Vector3.Dot(transform.forward, rb.velocity), 0.0f, 10000.0f);
        rb.AddForce(-200.0f * forwardVelocity * Time.fixedDeltaTime * transform.up);
    }
}
