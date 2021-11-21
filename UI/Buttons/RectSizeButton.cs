using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pigeon
{
    public class RectSizeButton : Button
    {
        [Space(10f)]
        public RectTransform rectTransform;
        protected Vector2 defaultSize;
        public Vector2 hoverSize;
        public Vector2 clickSize;
        public bool relative = true;

        protected void Reset()
        {
            if (!rectTransform)
            {
                rectTransform = transform as RectTransform;
            }
        }

        public override void Awake()
        {
            base.Awake();

            defaultSize = rectTransform.sizeDelta;
        }

        public virtual void SetDefaultSize(Vector2 value)
        {
            defaultSize = value;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            hovering = true;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimateThickness(hoverSize, hoverSpeed, easingFunctionHover);
            StartCoroutine(hoverCoroutine);

            OnHoverEnter?.Invoke();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            if (clicking)
            {
                clicking = false;
                OnClickUp?.Invoke();
            }

            hovering = false;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimateThickness(relative ? Vector2.zero : defaultSize, hoverSpeed, easingFunctionHover);
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
            hoverCoroutine = AnimateThickness(clickSize, clickSpeed, easingFunctionHover);
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
                hoverCoroutine = AnimateThickness(hoverSize, clickSpeed, easingFunctionHover);
            }
            StartCoroutine(hoverCoroutine);

            OnClickUp?.Invoke();
        }

        protected virtual IEnumerator AnimateThickness(Vector2 targetSize, float speed, EasingFunctions.EvaluateMode ease)
        {
            Vector2 initialSize = rectTransform.sizeDelta;
            if (relative)
            {
                targetSize += defaultSize;
            }

            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                rectTransform.sizeDelta = Vector2.LerpUnclamped(initialSize, targetSize, ease(time));

                yield return null;
            }
        }
    }
}