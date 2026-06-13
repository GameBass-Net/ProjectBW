/// <summary>
/// Project : Easy Build System
/// Class : BuildingUpgradeAction.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations.Actions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations.Actions
{
    [Serializable]
    public class BuildingUpgradeAction : IBuildingMenuSlotAction
    {
        [SerializeField] private int m_variantIndex = 1;

        private BuildingMenuUI m_menu;

        public int VariantIndex { get => m_variantIndex; set => m_variantIndex = value; }

        public void Initialize(BuildingSlotData owner, BuildingMenuUI menu) => m_menu = menu;

        public Texture2D GetIcon(Texture2D fallback) => fallback;

        public void Execute()
        {
            UpgradeBuildingState upgradeState =
                BuildingController.Instance?.GetState(BuildingMode.Upgrade) as UpgradeBuildingState;

            if (upgradeState != null)
            {
                BuildingController.Instance.SetMode(BuildingMode.Upgrade);
                upgradeState.SetTargetVariant(m_variantIndex);
            }

            m_menu?.CloseMenu();
        }
    }
}