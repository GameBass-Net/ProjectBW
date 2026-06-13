/// <summary>
/// Project : Easy Build System
/// Class : CategorySelectionAction.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions
{
    [Serializable]
    public class CategorySelectionAction : IBuildingMenuSlotAction
    {
        [SerializeField] private int m_categoryIndex = -1;
        [SerializeField] private bool m_closeMenu;

        private BuildingMenuUI m_menu;

        public int CategoryIndex { get => m_categoryIndex; set => m_categoryIndex = value; }

        public bool CloseMenu { get => m_closeMenu; set => m_closeMenu = value; }

        public void Initialize(BuildingSlotData owner, BuildingMenuUI menu) => m_menu = menu;

        public Texture2D GetIcon(Texture2D fallback) => fallback;

        public void Execute()
        {
            if (m_menu == null || m_menu.Categories == null || m_menu.Categories.Count == 0)
            {
                return;
            }

            int clampedIndex = Mathf.Clamp(m_categoryIndex, 0, m_menu.Categories.Count - 1);
            m_menu.SelectCategory(clampedIndex);
            if (m_closeMenu)
            {
                m_menu.CloseMenu();
            }
        }
    }
}