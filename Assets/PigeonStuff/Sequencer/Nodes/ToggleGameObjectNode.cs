using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Sets a GameObject's active state
    /// </summary>
    [SerializedNode("Toggle GameObject")]
    public class ToggleGameObjectNode : Node
    {
        internal enum State
        {
            Off, On, Toggle
        }

        [SerializeField] State setState;

        [SerializeField] OverrideGameObject gameObject;

        public override string Description => "Set a GameObject's active state.";

        internal override string GetPreviewValue() => setState.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            GameObject go = caller.GetOverride(gameObject, sequence, sequenceID);
            go.SetActive(setState != State.Off && (setState == State.On || !go.activeSelf));

            return null;
        }
    }
}