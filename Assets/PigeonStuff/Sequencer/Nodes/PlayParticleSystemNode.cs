using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Set the state of a particle system
    /// </summary>
    [SerializedNode("Play Particle System")]
    public class PlayParticleSystemNode : Node
    {
        [SerializeField] OverrideParticleSystem particleSystem;
        [SerializeField] [Tooltip("Stop the particle system instead of playing it?")] bool stopSystem;

        public override string Description => "Set the state of a particle system";

        internal override string GetPreviewValue() => stopSystem ? "Stop" : "Start";

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            var system = caller.GetOverride(particleSystem, sequence, sequenceID).value;
            
            if (stopSystem)
            {
                system.Stop();
            }
            else
            {
                system.Play();
            }

            return null;
        }
    }
}