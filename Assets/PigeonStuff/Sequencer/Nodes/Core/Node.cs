using System.Collections;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// An abstract Node in a Sequence.
    /// </summary>
    [System.Serializable]
    public abstract class Node
    {
        [SerializeField] string name;

        /// <summary>
        /// This node's name. Defaults to the node's type name but can be changed in the inspector.
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// Tooltip to show in the inspector
        /// </summary>
        public virtual string Description { get => null; }

        /// <summary>
        /// Override this to display a preview value in the inspector for nodes of this type.
        /// Default returns null, which causes no preview value to be shown.
        /// </summary>
        internal virtual string GetPreviewValue() => null;

        /// <summary>
        /// Iterator to run through when a sequence is playing.
        /// The returned sequence will be played in a coroutine.
        /// It should be possible to play multiple of the same sequence at the same time on different callers, so this iterator should NOT depend on any variables stored on this object.
        /// </summary>
        internal abstract IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID);

        /// <summary>
        /// This is called when a new instance of this node is created in a sequence.
        /// </summary>
        //internal virtual void OnCreate(Sequence sequence, int index) { }
    }
}