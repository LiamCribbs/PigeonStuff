using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Set an objects local or global position, rotation, and scale
    /// </summary>
    [SerializedNode("Transform/Set Transform")]
    public class SetTransformNode : Node
    {
        [SerializeField] OverrideVector3 position;
        [SerializeField] OverrideVector3 rotation;
        [SerializeField] OverrideVector3 scale = new OverrideVector3() { value = Vector3.one };
        [SerializeField] TransformSpace space = TransformSpace.Local;

        [SerializeField] OverrideTransform transform;

        public override string Description => "Set a transform's position, rotation, and scale";

        internal override string GetPreviewValue() => position?.value.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var transform = caller.GetOverride(this.transform, sequence, sequenceID).value;
            Vector3 position = caller.GetOverride(this.position, sequence, sequenceID).value;
            Quaternion rotation = Quaternion.Euler(caller.GetOverride(this.rotation, sequence, sequenceID).value);
            Vector3 scale = caller.GetOverride(this.scale, sequence, sequenceID).value;

            space.SetPosition(transform, position);
            space.SetRotation(transform, rotation);
            transform.localScale = scale;

            return null;
        }
    }
}