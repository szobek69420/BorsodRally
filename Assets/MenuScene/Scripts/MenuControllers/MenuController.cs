using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Canvas[] canvases;

    public virtual void Show()
    {
        foreach (Canvas c in canvases)
            c.gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        foreach (Canvas c in canvases)
            c.gameObject.SetActive(false);
    }
}
