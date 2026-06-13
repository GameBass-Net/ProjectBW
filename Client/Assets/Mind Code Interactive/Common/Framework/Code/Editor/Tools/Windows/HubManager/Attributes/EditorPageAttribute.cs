/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorPageAttribute.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorPageAttribute : Attribute
    {
        public string Id { get; }
        public string Category { get; }
        public string DisplayName { get; }
        public string IconPath { get; }
        public int Order { get; }
        public bool IsDefault { get; }

        public EditorPageAttribute(string pageId, string pageCategory, string pageDisplayName, string pageIconPath = null, int pageOrder = 0, bool pageIsDefault = false)
        {
            Id = pageId;
            DisplayName = pageDisplayName;
            Category = pageCategory;
            IconPath = pageIconPath;
            Order = pageOrder;
            IsDefault = pageIsDefault;
        }
    }
}