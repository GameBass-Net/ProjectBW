/// <summary>
/// Project : Mind Code Interactive
/// Class : AnimatorStateAttribute.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AnimatorStateAttribute : PropertyAttribute
    {
        public string SourceFieldName { get; }
        public string SpeedFieldName { get; }
        public float DefaultSpeed { get; }
        public string LayerFieldName { get; }

        public AnimatorStateAttribute(
            string sourceFieldName = null,
            string speedFieldName = null,
            string layerFieldName = null)
        {
            SourceFieldName = sourceFieldName;
            SpeedFieldName = speedFieldName;
            DefaultSpeed = 1f;
            LayerFieldName = layerFieldName;
        }
    }
}