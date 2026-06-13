/// <summary>
/// Project : Easy Build System
/// Class : CreateBuildingMenuCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class CreateBuildingMenuCommand : IMenuCommand
    {
        public const string TOOLS_PATH = "Tools/Mind Code Interactive/Easy Build System/UI/Create Building Catalog Menu...";
        public const string GAMEOBJECT_PATH = "GameObject/Mind Code Interactive/Easy Build System/UI/Create Building Catalog Menu...";
        public const int PRIORITY = 20;

        public const string CATALOG_MENU_PREFAB_PATH = "Building Menus/UI_BuildingCatalogMenu";
        public string MenuPath => GAMEOBJECT_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute()
        {
            GameObject prefab = Resources.Load<GameObject>(CATALOG_MENU_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError("Building Catalog Menu prefab not found at path: " + CATALOG_MENU_PREFAB_PATH);
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Building Catalog Menu UI";

#pragma warning disable CS0618
            Canvas canvas = Object.FindObjectOfType<Canvas>();
#pragma warning restore CS0618

            Undo.RegisterCreatedObjectUndo(instance, "Create Building Catalog Menu");

            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
            SceneView.lastActiveSceneView?.FrameSelected();

            Debug.Log("Building Catalog Menu UI successfully created.");
            EditorSceneManager.MarkAllScenesDirty();
        }
    }

    public class CreateBuildingRadialMenuCommand : IMenuCommand
    {
        public const string TOOLS_PATH = "Tools/Mind Code Interactive/Easy Build System/UI/Create Building Radial Menu...";
        public const string GAMEOBJECT_PATH = "GameObject/Mind Code Interactive/Easy Build System/UI/Create Building Radial Menu...";
        public const int PRIORITY = 21;

        public const string RADIAL_MENU_PREFAB_PATH = "Building Menus/UI_BuildingRadialMenu";

        public string MenuPath => GAMEOBJECT_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute()
        {
            GameObject prefab = Resources.Load<GameObject>(RADIAL_MENU_PREFAB_PATH);

            if (prefab == null)
            {
                Debug.LogError("Building Radial Menu prefab not found at path: " + RADIAL_MENU_PREFAB_PATH);
                return;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Building Radial Menu UI";

#pragma warning disable CS0618
            Canvas canvas = Object.FindObjectOfType<Canvas>();
#pragma warning restore CS0618

            if (canvas != null)
            {
                instance.transform.SetParent(canvas.transform, false);
            }

            Undo.RegisterCreatedObjectUndo(instance, "Create Building Radial Menu");

            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
            SceneView.lastActiveSceneView?.FrameSelected();

            Debug.Log("Building Radial Menu UI successfully created.");
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}