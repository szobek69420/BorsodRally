using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerOpponentMultiplayerHost : RacerBase
{
    protected override void RacerFixedUpdate()
    {
        //nothing
    }

    protected override void RacerUpdate()
    {
        //nothing
    }

    protected override void RacerOnFinish()
    {
        (gameManager as GameManagerMultiplayer).RegisterMultiplayerFinish(this.GetComponent<RacerId>().id);
    }
}
