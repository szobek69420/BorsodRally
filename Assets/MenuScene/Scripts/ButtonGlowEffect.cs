using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGlowEffect : MonoBehaviour
{
    [SerializeField] private TMP_FontAsset buttonNormalFont;
    [SerializeField] private TMP_FontAsset buttonHoveredFont;

    public void OnHoverStart(Button button)
    {
        button.gameObject.GetComponentInChildren<TMP_Text>().font = buttonHoveredFont;
    }

    public void OnHoverEnd(Button button)
    {
        button.gameObject.GetComponentInChildren<TMP_Text>().font = buttonNormalFont;
    }
}
