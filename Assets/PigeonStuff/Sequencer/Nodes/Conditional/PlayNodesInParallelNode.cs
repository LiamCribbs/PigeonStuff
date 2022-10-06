using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Plays n following nodes in parallel
    /// </summary>
    [SerializedNode("Conditional/Play Nodes In Parallel")]
    public class PlayNodesInParallelNode : Node
    {
        [SerializeField] [Tooltip("How many following nodes should be played in parallel")] int numberOfNodesToPlay;

        public override string Description => "Plays n following nodes in parallel";

        internal override string GetPreviewValue() => numberOfNodesToPlay.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            RefInt index = caller.NodeEnumerationIndex;
            int lastNodeToPlay = index.value + numberOfNodesToPlay;

            RefInt completedCoroutineCount = new RefInt(0);
            int startedCoroutines = 0;

            for (index.value++; index.value <= lastNodeToPlay; index.value++)
            {
                caller.NodeEnumerationIndex = index;

                IEnumerator nodeEnumerator = sequence.Nodes[index.value].Invoke(caller, sequence, sequenceID);
                if (nodeEnumerator != null)
                {
                    caller.StartCoroutine(Sequence.RunParallelCoroutine(nodeEnumerator, completedCoroutineCount));
                    startedCoroutines++;
                }

                while (completedCoroutineCount.value < startedCoroutines)
                {
                    yield return null;
                }
            }
        }
    }
}