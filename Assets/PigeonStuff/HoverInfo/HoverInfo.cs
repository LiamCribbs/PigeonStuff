using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class HoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void HoverInfoCallback(HoverInfo info);

    public static event HoverInfoCallback OnHoverEnter;
    public static event HoverInfoCallback OnHoverExit;

    public abstract string GetInfo();

    protected bool hovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        OnHoverEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        OnHoverExit?.Invoke(this);
    }

    protected virtual void OnDisable()
    {
        if (hovering)
        {
            hovering = false;
            OnHoverExit?.Invoke(this);
        }
    }
}