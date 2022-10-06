using System.Collections;
using UnityEngine;
using Pigeon.Math;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Animate an object's position, rotation, and scale from its current value to targets
    /// </summary>
    [SerializedNode("Transform/Animate Transform To")]
    public class AnimateTransformToNode : Node
    {
        [SerializeField] OverrideVector3 targetPosition;
        [Space(10)]
        [SerializeField] OverrideVector3 targetRotation;
        [Space(10)]
        [SerializeField] OverrideVector3 targetScale = new OverrideVector3() { value = Vector3.one };
        [Space(10)]
        [SerializeField] [Min(0f)] float speed = 1f;
        [SerializeField] EaseFunction easeFunction = new EaseFunction(EaseFunctions.EaseMode.Linear);
        [SerializeField] TransformSpace space = TransformSpace.Local;

        [SerializeField] OverrideTransform transform;

        public override string Description => "Animate a transform's position, rotation, and scale to values";
        
        internal override string GetPreviewValue() => targetPosition?.value.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var transform = caller.GetOverride(this.transform, sequence, sequenceID).value;

            Vector3 initialPosition = space.GetPosition(transform);
            Vector3 targetPosition = caller.GetOverride(this.targetPosition, sequence, sequenceID).value;
            Quaternion initialRotation = space.GetRotation(transform);
            Quaternion targetRotation = Quaternion.Euler(caller.GetOverride(this.targetRotation, sequence, sequenceID).value);
            Vector3 initialScale = transform.localScale;
            Vector3 targetScale = caller.GetOverride(this.targetScale, sequence, sequenceID).value;

            float time = 0f;

            while (time < 1f)
            {
                time += speed * Time.deltaTime;
                if (time > 1f)
                {
                    time = 1f;
                }

                space.SetPosition(transform, Vector3.LerpUnclamped(initialPosition, targetPosition, easeFunction.Evaluate(time)));
                space.SetRotation(transform, Quaternion.LerpUnclamped(initialRotation, targetRotation, easeFunction.Evaluate(time)));
                transform.localScale = Vector3.LerpUnclamped(initialScale, targetScale, easeFunction.Evaluate(time));

                yield return null;
            }
        }
    }
}