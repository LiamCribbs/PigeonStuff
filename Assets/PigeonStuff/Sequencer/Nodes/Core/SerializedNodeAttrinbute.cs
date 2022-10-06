using System;

namespace Pigeon.Sequencer
{
    /// <summary>
    /// Apply this attribute to a class that derives from Node to use it in a Sequencer.
    /// Path is the menu path to this node when adding a new node.
    /// Name is the default name when a new node of this type is created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class SerializedNodeAttribute : Attribute
    {
        public string Path { get; }
        public string Name { get; }

        /// <summary>
        /// If no name is provided, new nodes' names will default to the Path name
        /// </summary>
        public SerializedNodeAttribute(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// If no name is provided, new nodes' names will default to the Path name
        /// </summary>
        public SerializedNodeAttribute(string path, string name)
        {
            this.Path = path;
            this.Name = name;
        }
    }
}