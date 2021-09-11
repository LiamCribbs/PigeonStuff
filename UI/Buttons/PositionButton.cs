using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pigeon
{
    public class PositionButton : Button
    {
        [Space(10f)]
        public RectTransform mainGraphic;
        Vector2 defaultPosition;
        public Vector2 hoverPosition;
        public Vector2 clickPosition;

        protected void Reset()
        {
            if (!mainGraphic)
            {
                mainGraphic = GetComponent<RectTransform>();
            }
        }

        public override void Awake()
        {
            base.Awake();

            defaultPosition = mainGraphic.localPosition;
        }

        public virtual void SetDefaultOutlineThickness(Vector2 value)
        {
            defaultPosition = value;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            hovering = true;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimatePosition(hoverPosition, hoverSpeed);
            StartCoroutine(hoverCoroutine);

            OnHoverEnter?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            hovering = false;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimatePosition(Vector2.zero, hoverSpeed);
            StartCoroutine(hoverCoroutine);

            OnHoverExit?.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            clicking = true;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimatePosition(clickPosition, clickSpeed);
            StartCoroutine(hoverCoroutine);

            OnClickDown?.Invoke();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            clicking = false;
            SetToNull(hoverCoroutine);
            if (hovering)
            {
                hoverCoroutine = AnimatePosition(hoverPosition, clickSpeed);
            }
            StartCoroutine(hoverCoroutine);

            OnClickUp?.Invoke();
        }

        IEnumerator AnimatePosition(Vector2 targetPosition, float speed)
        {
            Vector2 initialThickness = mainGraphic.localPosition;
            targetPosition += defaultPosition;

            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                mainGraphic.localPosition = Vector2.LerpUnclamped(initialThickness, targetPosition, easingFunctionClick.Invoke(time));

                yield return null;
            }
        }
    }
}