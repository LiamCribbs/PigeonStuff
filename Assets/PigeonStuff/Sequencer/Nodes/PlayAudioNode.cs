using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Plays an audio clip
    /// </summary>
    [SerializedNode("Play Audio")]
    public class PlayAudioNode : Node
    {
        [SerializeField] AudioClip clip;

        [SerializeField] OverrideAudioSource audioSource;

        [SerializeField] [Tooltip("If enabled, this will call PlayOneShot() on the given audio source. " +
            "Otherwise it will set the audio source's audio clip to this clip and call Play().")] bool playOneShot;

        public override string Description => "Play an audio clip";

        internal override string GetPreviewValue() => clip ? clip.name : null;

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            AudioSource source = caller.GetOverride(audioSource, sequence, sequenceID);
            if (playOneShot)
            {
                source.PlayOneShot(clip);
            }
            else
            {
                source.clip = clip;
                source.Play();
            }

            return null;
        }
    }
}