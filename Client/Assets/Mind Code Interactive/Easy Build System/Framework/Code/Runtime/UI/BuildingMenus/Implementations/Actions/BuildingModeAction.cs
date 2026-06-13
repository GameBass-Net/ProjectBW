/// <summary>
/// Project : Easy Build System
/// Class : BuildingModeAction.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations.Actions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations.Actions
{
    [Serializable]
    public class BuildingModeAction : IBuildingMenuSlotAction
    {
        [SerializeField] private BuildingMode m_mode = BuildingMode.Placement;
        [SerializeField] private bool m_closeMenu = true;

        private BuildingMenuUI m_menu;

        public BuildingMode Mode { get => m_mode; set => m_mode = value; }

        public bool CloseMenu { get => m_closeMenu; set => m_closeMenu = value; }

        public void Initialize(BuildingSlotData owner, BuildingMenuUI menu) => m_menu = menu;

        public Texture2D GetIcon(Texture2D fallback) => fallback;

        public void Execute()
        {
            BuildingController.Instance?.SetMode(m_mode);

            if (m_closeMenu)
            {
                m_menu?.CloseMenu();
            }
        }
    }
}