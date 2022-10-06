using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Logs a message to the console using <see cref="Debug.Log"/>
    /// </summary>
    [SerializedNode("Log Message")]
    public class LogMessageNode : Node
    {
        [SerializeField] string message;

        public override string Description => "Logs a string to the console";

        internal override string GetPreviewValue() => message;

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            Debug.Log(message, caller);
            return null;
        }
    }
}