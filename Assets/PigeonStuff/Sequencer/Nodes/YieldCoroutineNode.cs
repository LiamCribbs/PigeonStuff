using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Runs a coroutine from another script
    /// </summary>
    [SerializedNode("Yield Coroutine")]
    public class YieldCoroutineNode : Node
    {
        [SerializeField] OverrideUnityEventRefIEnumerator coroutine;

        public override string Description => "Runs a coroutine from another script";

        internal override string GetPreviewValue() => coroutine.value.GetPersistentEventCount() > 0 ? coroutine.value.GetPersistentMethodName(0) : null;

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            RefIEnumerator refEnumerator = new RefIEnumerator();
            caller.GetOverride(coroutine, sequence, sequenceID).value?.Invoke(refEnumerator);
            return refEnumerator.enumerator;
        }
    }
}