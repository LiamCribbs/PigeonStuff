using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Set an objects local or global position
    /// </summary>
    [SerializedNode("Transform/Set Position")]
    public class SetPositionNode : Node
    {
        [SerializeField] OverrideVector3 position;
        [SerializeField] TransformSpace space = TransformSpace.Local;

        [SerializeField] OverrideTransform transform;

        public override string Description => "Set a transform's position";

        internal override string GetPreviewValue() => position?.value.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var transform = caller.GetOverride(this.transform, sequence, sequenceID);
            Vector3 position = caller.GetOverride(this.position, sequence, sequenceID).value;

            space.SetPosition(transform, position);

            return null;
        }
    }
}