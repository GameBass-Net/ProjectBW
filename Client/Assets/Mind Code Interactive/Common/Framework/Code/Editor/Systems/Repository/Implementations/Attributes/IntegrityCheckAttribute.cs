/// <summary>
/// Project : Mind Code Interactive
/// Class : IntegrityCheckAttribute.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Implementations.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class IntegrityCheckAttribute : Attribute
    {
        public int Priority { get; }

        public IntegrityCheckAttribute(int priority = 0) => Priority = priority;
    }
}