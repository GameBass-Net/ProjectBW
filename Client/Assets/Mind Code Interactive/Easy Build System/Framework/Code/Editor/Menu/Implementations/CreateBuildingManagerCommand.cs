/// <summary>
/// Project : Easy Build System
/// Class : CreateBuildingManagerCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class CreateBuildingManagerCommand : IMenuCommand
    {
        public const string TOOLS_PATH = "Tools/Mind Code Interactive/Easy Build System/Components/Create Building Manager...";
        public const string GAMEOBJECT_PATH = "GameObject/Mind Code Interactive/Easy Build System/Components/Create Building Manager...";
        public const int PRIORITY = 10;

        public string MenuPath => GAMEOBJECT_PATH;
        public int Priority => PRIORITY;

        public bool Validate()
        {
            if (BuildingManager.Instance)
            {
                Debug.LogWarning("Building Manager already exists in the scene.");
                return false;
            }

            return true;
        }

        public void Execute()
        {
            GameObject createdGameObject = new GameObject("Building Manager");
            Undo.RegisterCreatedObjectUndo(createdGameObject, "Create Building Manager");
            createdGameObject.AddComponent<BuildingManager>();
            Selection.activeGameObject = createdGameObject;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("Building Manager has been created in the scene.");
        }
    }
}