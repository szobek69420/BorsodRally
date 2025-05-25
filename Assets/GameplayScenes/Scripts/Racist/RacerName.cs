using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RacerName : MonoBehaviour
{
    [SerializeField] private TMP_Text text_name;

    public string Name
    {
        set { text_name.text = value; }
    }

    private void Update()
    {
        text_name.gameObject.transform.rotation = 
            Quaternion.LookRotation(
                text_name.gameObject.transform.position - Camera.main.transform.position
                );
    }
}
