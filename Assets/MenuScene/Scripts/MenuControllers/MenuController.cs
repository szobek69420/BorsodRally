using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Canvas[] canvases;

    public virtual void Show()
    {
        foreach (Canvas c in canvases)
            c.enabled = true;
    }

    public virtual void Hide()
    {
        foreach (Canvas c in canvases)
            c.enabled = false;
    }

    protected void PlayClickSound()
    {
        GameObject.Find("audio_click")?.GetComponent<AudioSource>()?.Play();
    }
}
