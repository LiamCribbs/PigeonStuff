using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Sets an array of GameObjects' active state
    /// </summary>
    [SerializedNode("Toggle Multiple GameObjects")]
    public class ToggleMultipleGameObjectsNode : Node
    {
        [SerializeField] ToggleGameObjectNode.State setState;

        [SerializeField] OverrideGameObjectArray gameObjects;

        public override string Description => "Sets an array of GameObjects' active state.";

        internal override string GetPreviewValue() => setState.ToString();

        internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
        {
            GameObject[] gameObjects = caller.GetOverride(this.gameObjects, sequence, sequenceID);

            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].SetActive(setState != ToggleGameObjectNode.State.Off && (setState == ToggleGameObjectNode.State.On || !gameObjects[i].activeSelf));
            }

            return null;
        }
    }
}