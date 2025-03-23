using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UpdateSpeedo : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text speedo;
    private Rigidbody rb;

    private void Start()
    {
        rb= GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (speedo != null)
            speedo.text = ((int)(3.6f*rb.velocity.magnitude)).ToString() + " kph";
    }
}
