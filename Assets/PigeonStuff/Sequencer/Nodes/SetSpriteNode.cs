using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Set the sprite of a sprite renderer
    /// </summary>
    [SerializedNode("Set Sprite")]
    public class SetSpriteNode : Node
    {
        [SerializeField] Sprite sprite;
        [SerializeField] OverrideSpriteRenderer renderer;

        public override string Description => "Set the sprite of a sprite renderer";

        internal override string GetPreviewValue() => sprite != null ? sprite.name : null;

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            caller.GetOverride(renderer, sequence, sequenceID).value.sprite = sprite;
            return null;
        }
    }
}