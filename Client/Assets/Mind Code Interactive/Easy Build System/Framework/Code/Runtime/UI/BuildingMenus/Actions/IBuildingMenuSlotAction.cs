/// <summary>
/// Project : Easy Build System
/// Class : IBuildingMenuSlotAction.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Actions
{
    public interface IBuildingMenuSlotAction
    {
        void Initialize(BuildingSlotData owner, BuildingMenuUI menu);

        Texture2D GetIcon(Texture2D fallback);

        void Execute();
    }
}