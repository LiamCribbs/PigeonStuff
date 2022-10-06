using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pigeon.Math;

namespace Pigeon.UI
{
    public class ColorButton : Button
    {
        [Space(10f)]
        public Graphic mainGraphic;
        protected Color defaultColor;
        public Color hoverColor;
        public Color clickColor;

        protected void Reset()
        {
            if (!mainGraphic)
            {
                mainGraphic = GetComponent<Graphic>();
            }
        }

        public override void Awake()
        {
            base.Awake();

            defaultColor = mainGraphic.color;
        }

        public virtual void SetDefaultColor(Color value)
        {
            defaultColor = value;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            hovering = true;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimateThickness(hoverColor, hoverSpeed, easingFunctionHover);
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
            hoverCoroutine = AnimateThickness(defaultColor, hoverSpeed, easingFunctionHover);
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
            hoverCoroutine = AnimateThickness(clickColor, clickSpeed, easingFunctionClick);
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
                hoverCoroutine = AnimateThickness(hoverColor, clickSpeed, easingFunctionClick);
            }
            StartCoroutine(hoverCoroutine);

            OnClickUp?.Invoke();
        }

        IEnumerator AnimateThickness(Color targetColor, float speed, EaseFunctions.EvaluateMode ease)
        {
            Color initialColor = mainGraphic.color;

            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                mainGraphic.color = Color.LerpUnclamped(initialColor, targetColor, ease(time));

                yield return null;
            }
        }
    }
}