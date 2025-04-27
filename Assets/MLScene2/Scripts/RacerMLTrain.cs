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
        
    }

    protected override void RacerUpdate()
    {
        ApplyBrakes();
        ApplyGas();
        ApplySteering();
    }

    protected override void RacerOnFinish()
    {
        agent.GoalReached();
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
