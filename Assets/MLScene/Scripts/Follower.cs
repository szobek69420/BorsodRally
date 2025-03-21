using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    [SerializeField] private Transform followee;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool lookAtFollowee;

    private void Update()
    {
        transform.position =
            followee.transform.position+
            offset.x * followee.right +
            offset.y * followee.up +
            offset.z * followee.forward;

        if(lookAtFollowee)
            transform.rotation = Quaternion.LookRotation(followee.transform.position - transform.position);
        else
            transform.rotation = followee.transform.rotation;
    }
}
