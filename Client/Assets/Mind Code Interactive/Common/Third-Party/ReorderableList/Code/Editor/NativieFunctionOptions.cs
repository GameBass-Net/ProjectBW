/// <summary>
/// Project : Mind Code Interactive
/// Class : NativeFunctionOptions.cs
/// Namespace : MindCodeInteractive.ReorderableList.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.ReorderableList.Code.Editor
{
    public struct NativeFunctionOptions : IEquatable<NativeFunctionOptions>
    {
        public bool Draggable { get; }
        public bool DisplayHeader { get; }
        public bool DisplayAddButton { get; }
        public bool DisplayRemoveButton { get; }

        public NativeFunctionOptions(bool isDraggable, bool shouldDisplayHeader = true, bool shouldDisplayAddButton = true, bool shouldDisplayRemoveButton = true)
        {
            Draggable = isDraggable;
            DisplayHeader = shouldDisplayHeader;
            DisplayAddButton = shouldDisplayAddButton;
            DisplayRemoveButton = shouldDisplayRemoveButton;
        }

        public static NativeFunctionOptions Default => new NativeFunctionOptions(isDraggable: true, shouldDisplayHeader: true, shouldDisplayAddButton: true, shouldDisplayRemoveButton: true);

        public bool Equals(NativeFunctionOptions otherOptions)
            => Draggable == otherOptions.Draggable
                && DisplayHeader == otherOptions.DisplayHeader
                && DisplayAddButton == otherOptions.DisplayAddButton
                && DisplayRemoveButton == otherOptions.DisplayRemoveButton;

        public override bool Equals(object comparisonObject)
            => comparisonObject is NativeFunctionOptions otherOptions && Equals(otherOptions);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * 23 + Draggable.GetHashCode();
                hashCode = hashCode * 23 + DisplayHeader.GetHashCode();
                hashCode = hashCode * 23 + DisplayAddButton.GetHashCode();
                hashCode = hashCode * 23 + DisplayRemoveButton.GetHashCode();
                return hashCode;
            }
        }
    }
}