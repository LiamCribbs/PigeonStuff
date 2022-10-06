using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Set an objects local or global scale
    /// </summary>
    [SerializedNode("Transform/Set Scale")]
    public class SetScaleNode : Node
    {
        [SerializeField] OverrideVector3 scale = new OverrideVector3() { value = Vector3.one };

        [SerializeField] OverrideTransform transform;

        public override string Description => "Set a transform's scale";

        internal override string GetPreviewValue() => scale?.value.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var transform = caller.GetOverride(this.transform, sequence, sequenceID);
            transform.value.localScale = caller.GetOverride(scale, sequence, sequenceID).value;

            return null;
        }
    }
}