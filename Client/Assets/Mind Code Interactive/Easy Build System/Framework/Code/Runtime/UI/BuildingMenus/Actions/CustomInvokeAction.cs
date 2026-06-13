/// <summary>
/// Project : Easy Build System
/// Class : CustomInvokeAction.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;
using UnityEngine.Events;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions
{
    [Serializable]
    public class CustomInvokeAction : IBuildingMenuSlotAction
    {
        [SerializeField] private UnityEvent m_onExecute;
        [SerializeField] private bool m_closeMenu;

        private BuildingMenuUI m_menu;

        public UnityEvent OnExecute { get => m_onExecute; set => m_onExecute = value; }

        public bool CloseMenu { get => m_closeMenu; set => m_closeMenu = value; }

        public void Initialize(BuildingSlotData owner, BuildingMenuUI menu) => m_menu = menu;

        public Texture2D GetIcon(Texture2D fallback) => fallback;

        public void Execute()
        {
            m_onExecute?.Invoke();
            if (m_closeMenu)
            {
                m_menu?.CloseMenu();
            }
        }
    }
}