using UnityEngine;
using UnityEngine.UI;

namespace Pigeon
{
    public abstract class OutlineGraphic : Graphic, IAnimatableValue<float>
    {
        [Min(0f)] public float thickness = 10f;

        public float GetValue()
        {
            return thickness;
        }

        public void SetValue(float value)
        {
            thickness = value;
            SetVerticesDirty();
        }
    }
}