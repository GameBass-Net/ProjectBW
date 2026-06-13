/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorTreeViewItem.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor.IMGUI.Controls;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data
{
#if UNITY_6000_3_OR_NEWER
#pragma warning disable CS0618
#endif
    public class EditorTreeViewItem : TreeViewItem
    {
        public TreePageData PageData;

        public EditorTreeViewItem(int itemId, TreePageData pageData) : base(itemId, pageData.Depth, pageData.DisplayName)
        {
            PageData = pageData;

            if (pageData.IconTexture != null)
            {
                icon = pageData.IconTexture;
            }
        }
    }
#if UNITY_6000_3_OR_NEWER
#pragma warning restore CS0618
#endif
}