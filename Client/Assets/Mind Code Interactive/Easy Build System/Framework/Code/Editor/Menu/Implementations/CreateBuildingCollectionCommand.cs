/// <summary>
/// Project : Easy Build System
/// Class : CreateBuildingCollectionCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.IO;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class CreateBuildingCollectionCommand : IMenuCommand
    {
        public const string TOOLS_PATH = "Tools/Mind Code Interactive/Easy Build System/Components/Scriptable Objects/Create Building Collection...";
        public const int PRIORITY = 13;

        public string MenuPath => TOOLS_PATH;
        public int Priority => PRIORITY;

        public bool Validate() => true;

        public void Execute()
        {
            string savedAssetPath = EditorUtility.SaveFilePanelInProject(
                "Create Building Collection",
                "BuildingCollection",
                "asset",
                "Choose a file name and location for the Building Collection asset");

            if (string.IsNullOrEmpty(savedAssetPath))
            {
                return;
            }

            BuildingCollection createdAsset = ScriptableObject.CreateInstance<BuildingCollection>();

            SerializedObject assetSerializedObject = new SerializedObject(createdAsset);
            SerializedProperty collectionNameProperty = assetSerializedObject.FindProperty("m_collectionName");
            if (collectionNameProperty != null)
            {
                collectionNameProperty.stringValue = Path.GetFileNameWithoutExtension(savedAssetPath);
                assetSerializedObject.ApplyModifiedProperties();
            }

            AssetDatabase.CreateAsset(createdAsset, savedAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = createdAsset;
            EditorGUIUtility.PingObject(createdAsset);

            Debug.Log("Building Collection asset created: " + savedAssetPath);
        }
    }
}