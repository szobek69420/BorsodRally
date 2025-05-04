using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacerFlippedNotifier : MonoBehaviour
{
    //this script should be besides the collider that resembles the cars top side

    [SerializeField] private RacerBase player;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer==6&&player!=null)//track
        {
            player.RacerOnUpsideDown();
        }
    }
}
