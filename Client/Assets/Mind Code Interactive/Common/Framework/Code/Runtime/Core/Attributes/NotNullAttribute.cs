/// <summary>
/// Project : Mind Code Interactive
/// Class : NotNullAttribute.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes
{
    public class NotNullAttribute : PropertyAttribute
    {
        public string ErrorMessage { get; }

        public NotNullAttribute(string errorMessage = "This field cannot be null")
        {
            ErrorMessage = errorMessage;
        }
    }
}