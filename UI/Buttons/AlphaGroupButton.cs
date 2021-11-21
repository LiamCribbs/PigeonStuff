using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pigeon
{
    public class AlphaGroupButton : Button
    {
        [Space(10f)]
        public CanvasGroup group;
        float defaultAlpha;
        public float hoverAlpha;
        public float clickAlpha;

        public override void Awake()
        {
            base.Awake();

            defaultAlpha = group.alpha;
        }

        public virtual void SetDefaultAlpha(float value)
        {
            defaultAlpha = value;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            hovering = true;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimateThickness(hoverAlpha, hoverSpeed, easingFunctionHover);
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
            hoverCoroutine = AnimateThickness(defaultAlpha, hoverSpeed, easingFunctionHover);
            StartCoroutine(hoverCoroutine);

            OnHoverExit?.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            //print(eventData?.position);
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            clicking = true;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimateThickness(clickAlpha, clickSpeed, easingFunctionClick);
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
                hoverCoroutine = AnimateThickness(hoverAlpha, clickSpeed, easingFunctionClick);
            }
            StartCoroutine(hoverCoroutine);

            OnClickUp?.Invoke();
        }

        IEnumerator AnimateThickness(float targetThickness, float speed, EasingFunctions.EvaluateMode ease)
        {
            float initialThickness = group.alpha;

            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                group.alpha = Mathf.LerpUnclamped(initialThickness, targetThickness, ease(time));

                yield return null;
            }
        }
    }
}