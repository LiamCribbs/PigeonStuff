using UnityEngine;
using UnityEngine.EventSystems;

namespace Pigeon.UI
{
    public class ColorButtonInstant : ColorButton
    {
        [SerializeField] protected bool onlyClickUpIfHovering;

        [SerializeField] bool playAudio = true;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            if (clicking)
            {
                return;
            }

            hovering = true;

            mainGraphic.color = hoverColor;

            OnHoverEnter?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            //if (clicking)
            //{
            //    clicking = false;
            //    OnClickUp?.Invoke();
            //}

            hovering = false;

            mainGraphic.color = clicking ? clickColor : defaultColor;

            OnHoverExit?.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            clicking = true;

            mainGraphic.color = clickColor;

            OnClickDown?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            clicking = false;

            mainGraphic.color = hovering ? hoverColor : defaultColor;

            if (!onlyClickUpIfHovering || (eventData != null && RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, eventData.position)))
            {
                OnClickUp?.Invoke();
            }
        }
    }
}