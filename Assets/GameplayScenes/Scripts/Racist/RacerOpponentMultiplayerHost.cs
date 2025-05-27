using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerOpponentMultiplayerHost : RacerBase
{
    protected override void RacerFixedUpdate()
    {
        if (gameManager?.State != GameManagerBase.GameState.RACE)
            return;

        ApplyDownforce();
    }

    protected override void RacerUpdate()
    {
        //nothing
    }

    protected override void RacerOnFinish()
    {
        (gameManager as GameManagerMultiplayer).RegisterMultiplayerFinish(this.GetComponent<RacerId>().id);
    }

    private void ApplyDownforce()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float forwardVelocity = Mathf.Clamp(Vector3.Dot(transform.forward, rb.velocity), 0.0f, 10000.0f);
        rb.AddForce(-200.0f * forwardVelocity * Time.fixedDeltaTime * transform.up);
    }
}
