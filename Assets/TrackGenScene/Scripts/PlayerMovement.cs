using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public Transform playerTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w")) {
            rb.AddRelativeForce(2000 * Time.deltaTime,0,0);
        }
        if (Input.GetKey("s"))
        {
            rb.AddRelativeForce(-2000 * Time.deltaTime, 0, 0);
        }
        if (Input.GetKey("a"))
        {
            //rb.AddRelativeForce(0, 0, -2000 * Time.deltaTime);
            rb.angularVelocity = new Vector3(0, -1, 0);
        }
        if (Input.GetKey("d"))
        {
            //rb.AddRelativeForce(0, 0, 2000 * Time.deltaTime);
            rb.angularVelocity = new Vector3(0, 1, 0);
        }
        if (Input.GetKey("r"))
        {
            playerTransform.position = new Vector3(-10, 5, 10);
            playerTransform.rotation = Quaternion.identity;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
