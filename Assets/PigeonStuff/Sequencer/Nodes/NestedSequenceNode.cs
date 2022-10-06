using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Runs a Sequence as a nested node
    /// </summary>
    [SerializedNode("Nested Sequence")]
    public class NestedSequenceNode : Node, INestedSequenceNode
    {
        [SerializeField] Sequence sequence;
        [SerializeField] [Min(-1)] [Tooltip("Number of times to play this sequence.\n\nSet to -1 to loop infinitely.\n\nSet to 1 to run once")] int loopCount = 1;

        [SerializeField] [HideInInspector] byte[] id = System.Guid.NewGuid().ToByteArray();

        public byte[] GetID() => id;

        public Sequence GetSequence() => sequence;

        public override string Description => "Play a sequence nested in this sequence";

        internal override string GetPreviewValue() => sequence != null ? sequence.name : null;

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence parentSequence, byte[] sequenceID)
        {
            RefInt index = new RefInt();
            
            if (loopCount < 0)
            {
                // Loop infinitely
                while (true)
                {
                    bool yielded = false;
                    for (index.value = 0; index.value < sequence.Nodes.Count; index.value++)
                    {
                        caller.NodeEnumerationIndex = index;

                        IEnumerator nodeEnumerator = sequence.Nodes[index.value].Invoke(caller, sequence, id);
                        if (nodeEnumerator != null)
                        {
                            yielded = true;
                            yield return nodeEnumerator;
                        }
                    }

                    // Wait one frame before looping if none if the nodes yielded
                    if (!yielded)
                    {
                        yield return null;
                    }
                }
            }
            else
            {
                if (sequence.Parallel)
                {
                    RefInt completedCoroutineCount = new RefInt();
                    
                    for (int loop = 0; loop < loopCount; loop++)
                    {
                        completedCoroutineCount.value = 0;
                        int startedCoroutines = 0;

                        for (index.value = 0; index.value < sequence.Nodes.Count; index.value++)
                        {
                            caller.NodeEnumerationIndex = index;

                            IEnumerator nodeEnumerator = sequence.Nodes[index.value].Invoke(caller, sequence, id);
                            if (nodeEnumerator != null)
                            {
                                caller.StartCoroutine(Sequence.RunParallelCoroutine(nodeEnumerator, completedCoroutineCount));
                                startedCoroutines++;
                            }
                        }

                        while (completedCoroutineCount.value < startedCoroutines)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    for (int loop = 0; loop < loopCount; loop++)
                    {
                        for (index.value = 0; index.value < sequence.Nodes.Count; index.value++)
                        {
                            caller.NodeEnumerationIndex = index;

                            IEnumerator nodeEnumerator = sequence.Nodes[index.value].Invoke(caller, sequence, id);
                            if (nodeEnumerator != null)
                            {
                                yield return nodeEnumerator;
                            }
                        }
                    }
                }
            }
        }
    }
}