using System.Collections;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Destroy a GameObject
    /// </summary>
    [SerializedNode("Destroy GameObject")]
    public class DestroyGameObjectNode : Node
    {
        [SerializeField] OverrideGameObject gameObject;

        public override string Description => "Destroy a GameObject";

        internal override string GetPreviewValue() => gameObject?.value != null ? gameObject.value.name : null;

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            Object.Destroy(caller.GetOverride(gameObject, sequence, sequenceID).value);
            return null;
        }
    }
}