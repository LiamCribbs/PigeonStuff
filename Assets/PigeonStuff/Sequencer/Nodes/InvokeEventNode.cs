using System.Collections;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Invoke an event
    /// </summary>
    [SerializedNode("Invoke Event")]
    public class InvokeEventNode : Node
    {
        [SerializeField] OverrideUnityEventSequencePlayer invokeEvent;

        public override string Description => "Invoke an event";

        internal override string GetPreviewValue() => invokeEvent.value.GetPersistentEventCount() > 0 ? invokeEvent.value.GetPersistentMethodName(0) : null;

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            caller.GetOverride(invokeEvent, sequence, sequenceID).value?.Invoke(caller);
            return null;
        }
    }
}