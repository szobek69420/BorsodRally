using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//notifies the ml controller that the car has flipped
public class CarRoof : MonoBehaviour
{
    [SerializeField] private MLTrainController agent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6)//track
            agent.Dieded();
    }
}
