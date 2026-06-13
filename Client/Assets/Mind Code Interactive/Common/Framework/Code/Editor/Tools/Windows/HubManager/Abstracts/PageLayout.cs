/// <summary>
/// Project : Mind Code Interactive
/// Class : PageLayout.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts
{
    public abstract class PageLayout
    {
        public Vector2 ScrollPosition;

        public abstract void DrawLayout();

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }
    }
}