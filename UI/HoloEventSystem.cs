using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoloEventSystem : MonoBehaviour
{
    public GraphicRaycaster raycaster;
    IEventSystemHandler selectedGraphic;

    public State state;

    public enum State
    {
        None, Hover, Click
    }

    const float ParabolaCurve = 0.3f;

    public static Vector2 ScreenPositionInParabola(Vector2 position)
    {
        position.x /= Screen.width;
        position.y /= Screen.height;

        position.y += ParabolaCurve * (-2f * position.y + 1f) * position.x * (position.x - 1f);

        position.x *= Screen.width;
        position.y *= Screen.height;

        return position;
    }

    void Update()
    {
        if (!(Object)selectedGraphic)
        {
            selectedGraphic = null;
        }

        bool foundGraphic = false;
        bool selectedGraphicIsDifferent = false;
        IEventSystemHandler newGraphic = null;

        Vector2 position = ScreenPositionInParabola(Input.mousePosition);

        PointerEventData eventData = new PointerEventData(null)
        {
            position = position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i].gameObject.TryGetComponent(out IEventSystemHandler button))
            {
                selectedGraphicIsDifferent = button != selectedGraphic;
                if (selectedGraphicIsDifferent)
                {
                    newGraphic = button;
                }

                foundGraphic = true;
                break;
            }
        }

        if (foundGraphic)
        {
            if (selectedGraphicIsDifferent && state != State.Click)
            {
                if (selectedGraphic != null)
                {
                    ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
                }

                selectedGraphic = newGraphic;

                ((IPointerEnterHandler)selectedGraphic).OnPointerEnter(null);

                state = State.Hover;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ((IPointerDownHandler)selectedGraphic).OnPointerDown(null);
                state = State.Click;
            }
            else if (state == State.Click && Input.GetKeyUp(KeyCode.Mouse0))
            {
                ((IPointerUpHandler)selectedGraphic).OnPointerUp(null);
                state = State.Hover;
            }
        }
        else
        {
            if (state == State.Click && Input.GetKeyUp(KeyCode.Mouse0))
            {
                ((IPointerUpHandler)selectedGraphic).OnPointerUp(null);
                state = State.None;
            }
            
            if (state != State.Click && selectedGraphic != null)
            {
                ((IPointerExitHandler)selectedGraphic).OnPointerExit(null);
                selectedGraphic = null;
                state = State.None;
            }
        }
    }
}
