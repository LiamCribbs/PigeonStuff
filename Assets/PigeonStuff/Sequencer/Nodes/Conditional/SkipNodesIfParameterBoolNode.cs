using System.Collections;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Skip a number of nodes if a SequencePlayer parameter is true
    /// </summary>
    [SerializedNode("Conditional/Skip Nodes if Parameter is True")]
    public class SkipNodesIfParameterBoolNode : Node
    {
        [SerializeField] string parameter;
        [SerializeField] bool invert;
        [SerializeField] int nodesToSkip;

        public override string Description => "Invoke an event";

        internal override string GetPreviewValue() => parameter + " -> " + nodesToSkip.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            if (caller.GetParameter<bool>(parameter) ^ invert)
            {
                caller.IncreaseEnumerationIndex(nodesToSkip);
            }

            return null;
        }
    }
}