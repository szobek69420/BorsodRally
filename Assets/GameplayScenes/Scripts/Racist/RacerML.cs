using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerML : RacerBase
{
    [SerializeField] private MLController agent;
    [SerializeField] private IngameCarController carController;

    protected override void RacerUpdate()
    {
        ApplyBrakes();
        ApplyGas();
        ApplySteering();
    }

    protected override void RacerFixedUpdate()
    {
        //nothing to do here
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
}
