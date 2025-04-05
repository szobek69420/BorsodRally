using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerPlayer : RacerBase
{
    protected override void ApplyBrakes()
    {
        carController.BrakeInput = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    }

    protected override void ApplyGas()
    {
        carController.AccelInput = Input.GetAxisRaw("Vertical");
    }

    protected override void ApplySteering()
    {
        carController.SteerInput = Input.GetAxisRaw("Horizontal");
    }
}
