/// <summary>
/// Project : Mind Code Interactive
/// Class : ShowIfAttribute.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        public enum DisablingType
        {
            ReadOnly = 2,
            DontDraw = 3
        }

        public string ComparedPropertyName { get; }
        public object ComparedValue { get; }
        public DisablingType DisablingBehavior { get; }

        public ShowIfAttribute(
            string comparedPropertyName,
            object comparedValue,
            DisablingType disablingType = DisablingType.DontDraw)
        {
            ComparedPropertyName = comparedPropertyName;
            ComparedValue = comparedValue;
            DisablingBehavior = disablingType;
        }
    }
}