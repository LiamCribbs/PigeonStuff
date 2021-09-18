using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pigeon
{
    public class OutlineThicknessButton : Button
    {
        [Space(10f)]
        public OutlineGraphic mainGraphic;
        float defaultOutlineThickness;
        public float hoverOutlineThickness;
        public float clickOutlineThickness;

        [Space(10)]
        public bool playAudio = true;

        protected void Reset()
        {
            if (!mainGraphic)
            {
                mainGraphic = GetComponent<OutlineGraphic>();
            }
        }

        public override void Awake()
        {
            base.Awake();

            defaultOutlineThickness = mainGraphic.GetValue();
        }

        public virtual void SetDefaultOutlineThickness(float value)
        {
            defaultOutlineThickness = value;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (ignoreEvents && eventData != null)
            {
                return;
            }

            hovering = true;
            SetToNull(hoverCoroutine);
            hoverCoroutine = AnimateThickness(hoverOutlineThickness, hoverSpeed);
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
            hoverCoroutine = AnimateThickness(defaultOutlineThickness, hoverSpeed);
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
            hoverCoroutine = AnimateThickness(clickOutlineThickness, clickSpeed);
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
                hoverCoroutine = AnimateThickness(hoverOutlineThickness, clickSpeed);
            }
            StartCoroutine(hoverCoroutine);

            OnClickUp?.Invoke();
        }

        IEnumerator AnimateThickness(float targetThickness, float speed)
        {
            float initialThickness = mainGraphic.GetValue();

            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                mainGraphic.SetValue(Mathf.LerpUnclamped(initialThickness, targetThickness, easingFunctionClick.Invoke(time)));

                yield return null;
            }
        }

        public void Toggle()
        {
            if (clicking)
            {
                OnPointerUp(null);
                OnPointerExit(null);
            }
            else
            {
                OnPointerEnter(null);
                OnPointerDown(null);
            }
        }
    }
}