using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerClientPositionUpdate : MonoBehaviour
{
    private IngameCarComponents carComponents;

    private void Start()
    {
        carComponents=this.gameObject.GetComponent<IngameCarComponents>();
    }

    private void FixedUpdate()
    {
        if (carComponents == null)
            return;

        carComponents.ExtrapolateOrientation();
    }
}
