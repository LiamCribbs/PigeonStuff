using System.Collections;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Skip a number of nodes
    /// </summary>
    [SerializedNode("Conditional/Skip Nodes")]
    public class SkipNodesNode : Node
    {
        [SerializeField] int nodesToSkip;

        public override string Description => "Skip a number of nodes";

        internal override string GetPreviewValue() => nodesToSkip.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            caller.IncreaseEnumerationIndex(nodesToSkip);
            return null;
        }
    }
}