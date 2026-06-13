/// <summary>
/// Project : Easy Build System
/// Class : CreateBuildingSocketCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class CreateBuildingSocketCommand : IMenuCommand
    {
        public const string GAMEOBJECT_PATH = "GameObject/Mind Code Interactive/Easy Build System/Components/Create Building Socket...";
        public const string TOOLS_PATH = "Tools/Mind Code Interactive/Easy Build System/Components/Create Building Socket...";
        public const int PRIORITY = 12;

        public string MenuPath => GAMEOBJECT_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute()
        {
            GameObject createdGameObject = new GameObject("New Building Socket");
            Undo.RegisterCreatedObjectUndo(createdGameObject, "Create Building Socket");

            GameObject selectedParentGameObject = Selection.activeGameObject;
            if (selectedParentGameObject != null)
            {
                Undo.SetTransformParent(createdGameObject.transform, selectedParentGameObject.transform, "Parent Building Socket");
            }

            createdGameObject.transform.localPosition = Vector3.zero;
            createdGameObject.AddComponent<BuildingSocket>();

            Selection.activeGameObject = createdGameObject;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("Building socket created in the scene.");
        }
    }
}