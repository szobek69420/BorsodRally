using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyUIElementHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text text_name;
    [SerializeField] private TMP_Text text_number;

    public void SetInfo(string name, int number)
    {
        text_name.text = name;
        text_number.text=number.ToString();
    }
}
