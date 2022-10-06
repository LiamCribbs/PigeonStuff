using System.Collections;
using UnityEngine;
using Pigeon.Math;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Animate an object's rotation from its current value to a target
    /// </summary>
    [SerializedNode("Transform/Animate Rotation To")]
    public class AnimateRotationToNode : Node
    {
        [SerializeField] OverrideVector3 target;
        [SerializeField] [Min(0f)] float speed = 1f;
        [SerializeField] EaseFunction easeFunction = new EaseFunction(EaseFunctions.EaseMode.Linear);
        [SerializeField] TransformSpace space = TransformSpace.Local;

        [SerializeField] OverrideTransform transform;

        public override string Description => "Animate a transform's rotation to a value";
        
        internal override string GetPreviewValue() => target?.value.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var transform = caller.GetOverride(this.transform, sequence, sequenceID).value;
            Quaternion initial = space.GetRotation(transform);
            Quaternion target = Quaternion.Euler(caller.GetOverride(this.target, sequence, sequenceID).value);
            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                space.SetRotation(transform, Quaternion.LerpUnclamped(initial, target, easeFunction.Evaluate(time)));

                yield return null;
            }
        }
    }
}