using System.Collections;
using UnityEngine;
using Pigeon.Sequencer;

/// <summary>
/// This is an event node, so its path should start with "Event/".
/// The second string is the default name of any nodes of this type. If no argument is given the default name becomes the last part of the path.
/// </summary>
[SerializedNode("Custom/Example Custom Node", "Special Custom Node Name")]
public class ExampleCustomNode : Node
{
    /// <summary>
    /// This can be edited in the Sequencer inspector
    /// </summary>
    [SerializeField] string stringToPrint = "I print to the console";

    /// <summary>
    /// This string's value can be overrided in a <see cref="SequencePlayer"/>. New overrideable types should inheret from <see cref="OverrideField{T}"/>.
    /// For a new OverrideField type to be properly serialized in the inspector, it must be declared as its own class rather than a generic.
    /// For example, we need to declare <see cref="OverrideString"/> as a class inhereting from OverrideField{string} rather than creating an OverrideField{string} field.
    /// </summary>
    [SerializeField] OverrideString overrideStringToPrint;

    /// <summary>
    /// This Description property returns a tooltip that appears when hovering over this node in the inspector
    /// </summary>
    public override string Description => "This is an example node. The Description property appears as a tooltip in the inspector.";

    /// <summary>
    /// GetPreviewValue is overrided to return stringToPrint.
    /// This will display stringToPrint to the right of this node in the Sequencer inspector.
    /// </summary>
    internal override string GetPreviewValue() => stringToPrint;

    /// <summary>
    /// Invoke is called when this node is reached in a sequence that's being played.
    /// Here we just print stringToPrint to the console.
    /// Because this node isn't doing any iterating over time, we can return null to let the caller know to move on to the next node.
    /// If we wanted to invoke over time, we could make this method look like a standard coroutine iterator block.
    /// </summary>
    internal override IEnumerator Invoke(SequencePlayer caller, Sequence sequence, byte[] sequenceID)
    {
        Debug.Log(stringToPrint);

        // Pass any override values into caller.GetOverride before using them.
        // This method will return the overrided value, or the given value if it isn't overrided.
        Debug.Log(caller.GetOverride(overrideStringToPrint, sequence, sequenceID).value);

        return null;
    }
}