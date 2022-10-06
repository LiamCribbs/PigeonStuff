using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Waits for seconds
    /// </summary>
    [SerializedNode("Wait Seconds")]
    public class WaitSecondsNode : Node
    {
        [SerializeField] [Min(0f)] float seconds;

        public override string Description => "Pause sequence for x seconds";

        internal override string GetPreviewValue() => seconds.ToString() + "s";

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            float time = 0f;
            while (time < seconds)
            {
                time += Time.deltaTime;
                yield return null;
            }
        }
    }
}