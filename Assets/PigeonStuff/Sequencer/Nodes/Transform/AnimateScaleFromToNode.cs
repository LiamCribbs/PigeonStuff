using System.Collections;
using UnityEngine;
using Pigeon.Math;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Animate an object's scale from an initial value to a target
    /// </summary>
    [SerializedNode("Transform/Animate Scale From To")]
    public class AnimateScaleFromToNode : Node
    {
        [SerializeField] OverrideVector3 initial = new OverrideVector3() { value = Vector3.one };
        [SerializeField] OverrideVector3 target = new OverrideVector3() { value = Vector3.one };
        [SerializeField] [Min(0f)] float speed = 1f;
        [SerializeField] EaseFunction easeFunction = new EaseFunction(EaseFunctions.EaseMode.Linear);

        [SerializeField] OverrideTransform transform;

        public override string Description => "Animate a transform's scale from a value to a target";

        internal override string GetPreviewValue() => target?.value.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var transform = caller.GetOverride(this.transform, sequence, sequenceID).value;
            Vector3 initial = caller.GetOverride(this.initial, sequence, sequenceID).value;
            Vector3 target = caller.GetOverride(this.target, sequence, sequenceID).value;
            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                transform.localScale = Vector3.LerpUnclamped(initial, target, easeFunction.Evaluate(time));

                yield return null;
            }
        }
    }
}