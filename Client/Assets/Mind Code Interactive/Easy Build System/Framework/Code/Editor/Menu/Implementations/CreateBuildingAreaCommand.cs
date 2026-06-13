/// <summary>
/// Project : Easy Build System
/// Class : CreateBuildingAreaCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Areas;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class CreateBuildingAreaCommand : IMenuCommand
    {
        public const string TOOLS_PATH = "Tools/Mind Code Interactive/Easy Build System/Components/Create Building Area...";
        public const string GAMEOBJECT_PATH = "GameObject/Mind Code Interactive/Easy Build System/Components/Create Building Area...";
        public const int PRIORITY = 10;

        public string MenuPath => GAMEOBJECT_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute()
        {
            GameObject createdGameObject = new GameObject("Building Area");
            Undo.RegisterCreatedObjectUndo(createdGameObject, "Create Building Area");
            createdGameObject.AddComponent<BuildingArea>();
            Selection.activeGameObject = createdGameObject;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("Building Area has been created in the scene.");
        }
    }
}