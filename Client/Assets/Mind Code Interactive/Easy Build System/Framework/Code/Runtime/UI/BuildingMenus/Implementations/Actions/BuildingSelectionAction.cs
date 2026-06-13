/// <summary>
/// Project : Easy Build System
/// Class : BuildingSelectionAction.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations.Actions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Attributes;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations.Actions
{
    [Serializable]
    public class BuildingSelectionAction : IBuildingMenuSlotAction
    {
        [SerializeField, BuildingPartReference] private string m_partReference;
        [SerializeField] private bool m_closeMenu = true;

        private BuildingMenuUI m_menu;

        public string PartReference { get => m_partReference; set => m_partReference = value; }

        public bool CloseMenu { get => m_closeMenu; set => m_closeMenu = value; }

        private BuildingPart BuildingPart =>
            !string.IsNullOrEmpty(m_partReference) && BuildingPartRegistry.Instance != null
                ? BuildingPartRegistry.Instance.GetPartByPrefabId(m_partReference)
                : null;

        public void Initialize(BuildingSlotData owner, BuildingMenuUI menu) => m_menu = menu;

        public Texture2D GetIcon(Texture2D fallback)
        {
            Texture2D partThumbnail = BuildingPart != null ? BuildingPart.Thumbnail : null;
            return partThumbnail != null ? partThumbnail : fallback;
        }

        public void Execute()
        {
            BuildingPart part = BuildingPart;
            if (part == null)
            {
                return;
            }

            BuildingController.Instance?.SelectPart(part);
            BuildingController.Instance?.SetMode(BuildingMode.Placement);

            if (m_closeMenu)
            {
                m_menu?.CloseMenu();
            }
        }
    }
}