using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedometerHandler : MonoBehaviour
{
    [SerializeField] private Image image_speedoLevel;
    [SerializeField] private TMP_Text text_speed;

    //speed is in m/s
    public void SetSpeed(float speed)
    {
        text_speed.text = ((int)(speed * 3.6f)).ToString();
        image_speedoLevel.fillAmount= 0.625f * Mathf.Clamp(speed / 70.0f, 0.0f, 1.0f);
    }
}
