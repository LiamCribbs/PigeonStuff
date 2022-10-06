using System.Collections;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Exit a sequence early
    /// </summary>
    [SerializedNode("Exit")]
    public class ExitNode : Node
    {
        public override string Description => "Exit a sequence early";

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            caller.Stop();
            return null;
        }
    }
}