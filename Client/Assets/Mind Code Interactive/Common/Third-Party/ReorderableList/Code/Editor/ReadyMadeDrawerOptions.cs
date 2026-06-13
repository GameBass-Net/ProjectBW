/// <summary>
/// Project : Mind Code Interactive
/// Class : ReadyMadeDrawerOptions.cs
/// Namespace : MindCodeInteractive.ReorderableList.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.ReorderableList.Code.Editor
{
    public struct ReadyMadeDrawerOptions : IEquatable<ReadyMadeDrawerOptions>
    {
        public bool UseReadyMadeHeader { get; }
        public bool UseReadyMadeElement { get; }
        public bool UseReadyMadeBackground { get; }

        public ReadyMadeDrawerOptions(bool shouldUseReadyMadeHeader, bool shouldUseReadyMadeElement = true, bool shouldUseReadyMadeBackground = true)
        {
            UseReadyMadeHeader = shouldUseReadyMadeHeader;
            UseReadyMadeElement = shouldUseReadyMadeElement;
            UseReadyMadeBackground = shouldUseReadyMadeBackground;
        }

        public static ReadyMadeDrawerOptions Default => new ReadyMadeDrawerOptions(shouldUseReadyMadeHeader: true, shouldUseReadyMadeElement: true, shouldUseReadyMadeBackground: true);

        public bool Equals(ReadyMadeDrawerOptions otherOptions)
            => UseReadyMadeHeader == otherOptions.UseReadyMadeHeader
                && UseReadyMadeElement == otherOptions.UseReadyMadeElement
                && UseReadyMadeBackground == otherOptions.UseReadyMadeBackground;

        public override bool Equals(object comparisonObject)
            => comparisonObject is ReadyMadeDrawerOptions otherOptions && Equals(otherOptions);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * 23 + UseReadyMadeHeader.GetHashCode();
                hashCode = hashCode * 23 + UseReadyMadeElement.GetHashCode();
                hashCode = hashCode * 23 + UseReadyMadeBackground.GetHashCode();
                return hashCode;
            }
        }
    }
}