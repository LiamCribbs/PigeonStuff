using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Set an objects local or global rotation
    /// </summary>
    [SerializedNode("Transform/Set Rotation", "Set Rotation")]
    public class SetRotationNode : Node
    {
        [SerializeField] OverrideVector3 rotation;
        [SerializeField] TransformSpace space = TransformSpace.Local;

        [SerializeField] OverrideTransform transform;

        public override string Description => "Set a transform's rotation";

        internal override string GetPreviewValue() => rotation?.value.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var transform = caller.GetOverride(this.transform, sequence, sequenceID);
            Quaternion rotation = Quaternion.Euler(caller.GetOverride(this.rotation, sequence, sequenceID).value);

            space.SetRotation(transform, rotation);

            return null;
        }
    }
}