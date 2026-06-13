/// <summary>
/// Project : Easy Build System
/// Class : BuildingSlotData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data
{
    [Serializable]
    public class BuildingSlotData
    {
        [SerializeField] private string m_name;
        [SerializeField] private string m_description;
        [SerializeField] private Texture2D m_icon;
        [SerializeReference] private IBuildingMenuSlotAction m_action;

        private BuildingMenuUI m_menu;

        public string Name { get => m_name; set => m_name = value; }

        public string Description { get => m_description; set => m_description = value; }

        public Texture2D Icon { get => m_icon; set => m_icon = value; }

        public IBuildingMenuSlotAction Action => m_action;

        public virtual bool IsAvailable => true;

        public virtual int? RemainingCount => null;

        public virtual Texture2D GetIcon()
        {
            IBuildingMenuSlotAction action = m_action;
            return action != null ? action.GetIcon(m_icon) : m_icon;
        }

        public void EnsureAction(BuildingMenuUI menu)
        {
            if (!ReferenceEquals(m_menu, menu))
            {
                m_menu = menu;
            }

            m_action?.Initialize(this, m_menu);
        }

        public virtual void Execute()
        {
            if (!IsAvailable)
            {
                return;
            }

            m_action?.Execute();
        }

        public virtual bool ConsumeOne()
        {
            return true;
        }
    }
}