/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorCategoryAttribute.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorCategoryAttribute : Attribute
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string IconPath { get; }
        public int Order { get; }

        public EditorCategoryAttribute(string categoryId, string categoryDisplayName, string categoryIconPath = null, int categoryOrder = 0)
        {
            Id = categoryId;
            DisplayName = categoryDisplayName;
            IconPath = categoryIconPath;
            Order = categoryOrder;
        }
    }
}