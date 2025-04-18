using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameCarComponents : MonoBehaviour
{
    public Transform car;

    public Transform wheelFl;
    public Transform wheelFr;
    public Transform wheelRl;
    public Transform wheelRr;

    public Rigidbody rb;

    //sets the ownerId to 0
    public CarOrientation GetOrientation()
    {
        return new CarOrientation(0, car, wheelFl, wheelFr, wheelRl, wheelRr, rb.velocity);
    }
}
