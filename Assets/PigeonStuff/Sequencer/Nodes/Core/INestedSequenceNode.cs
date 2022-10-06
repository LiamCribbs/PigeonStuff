using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Implement this interface in any node that plays a nested <see cref="Sequence"/>
    /// </summary>
    public interface INestedSequenceNode
    {
        /// <summary>
        /// Get the nested sequence
        /// </summary>
        Sequence GetSequence();

        /// <summary>
        /// (EDITOR ONLY) Get this node's unique id.
        /// The id should be a serialized byte[] field that gets set to <see cref="System.Guid.NewGuid().ToByteArray()"/> on initialization
        /// </summary>
        byte[] GetID();

        public static bool AreIDsEqual(byte[] a, byte[] b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null)
            {
                if (b.Length == 0)
                {
                    return true;
                }

                return false;
            }

            if (b == null)
            {
                if (a.Length == 0)
                {
                    return true;
                }

                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}