using System.Collections;
using UnityEngine;
using Pigeon.Math;

namespace Pigeon.UI
{
    public class RectSizeButtonActive : RectSizeButton
    {
        public override void SetHover(bool hover)
        {
            if (hover)
            {
                gameObject.SetActive(true);
            }

            base.SetHover(hover);
        }

        protected override IEnumerator AnimateThickness(Vector2 targetSize, float speed, EaseFunctions.EvaluateMode ease)
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

            if (targetSize.y == 0f)
            {
                gameObject.SetActive(false);
            }
        }
    }
}