/// <summary>
/// Project : Easy Build System
/// Class : BuildingCategoryData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data
{
    [Serializable]
    public class BuildingCategoryData
    {
        [SerializeField] private string m_name = "New Category";
        [SerializeField] private List<BuildingSlotData> m_slots = new List<BuildingSlotData>();

        public string Name { get => m_name; set => m_name = value; }

        public List<BuildingSlotData> Slots { get => m_slots; set => m_slots = value; }
    }
}