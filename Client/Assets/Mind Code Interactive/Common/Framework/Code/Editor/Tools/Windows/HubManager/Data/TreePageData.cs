/// <summary>
/// Project : Mind Code Interactive
/// Class : TreePageData.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Abstracts;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Tools.Windows.HubManager.Data
{
    [Serializable]
    public class TreePageData
    {
        public string Id;
        public string DisplayName;
        public string IconPath;

        [NonSerialized] public Texture2D IconTexture;
        [NonSerialized] public TreePageData Parent;
        [NonSerialized] public List<TreePageData> Children = new List<TreePageData>();
        [NonSerialized] public Type PageLayoutType;
        [NonSerialized] public int Depth;

        public bool IsExpanded = true;
        public bool IsCategory;
        public bool IsDefault;

        public TreePageData(string pageId, string pageDisplayName, string pageIconPath = null, Type pageLayoutType = null, bool pageIsCategory = false, bool pageIsDefault = false)
        {
            Id = pageId;
            DisplayName = pageDisplayName;
            IconPath = pageIconPath;
            PageLayoutType = pageLayoutType;
            IsCategory = pageIsCategory;
            IsDefault = pageIsDefault;
            Children = new List<TreePageData>();
            LoadIcon();
        }

        public void AddChild(TreePageData childPageData)
        {
            if (Children == null)
            {
                Children = new List<TreePageData>();
            }

            childPageData.Parent = this;
            childPageData.Depth = Depth + 1;
            Children.Add(childPageData);
        }

        public PageLayout CreatePageLayout()
            => PageLayoutType != null && typeof(PageLayout).IsAssignableFrom(PageLayoutType)
                ? (PageLayout)Activator.CreateInstance(PageLayoutType)
                : null;

        private void LoadIcon()
        {
            if (!string.IsNullOrEmpty(IconPath))
            {
                IconTexture = Resources.Load<Texture2D>(IconPath);
                if (IconTexture == null)
                {
                    Debug.LogWarning("Icon not found at path: Resources/" + IconPath);
                }
            }
        }
    }
}