using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerCamera : MonoBehaviour
{

    public Transform playerTransform;
    public Vector3 offset;
    public Vector3 rotOffset;

    // Start is called before the first frame update
    void Start()
    {
          
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = playerTransform.localPosition + offset.x*playerTransform.right + offset.y*playerTransform.up + offset.z*playerTransform.forward;
        //transform.rotation = Quaternion.LookRotation(playerTransform.position - transform.position, playerTransform.up);
    }
}
