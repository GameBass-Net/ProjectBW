/// <summary>
/// Project : Easy Build System
/// Class : QuickSetupCommand.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Systems.MenuCommand.Interfaces;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Inputs;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Menu.Implementations
{
    public class QuickSetupCommand : IMenuCommand
    {
        public const string MENU_PATH = "Tools/Mind Code Interactive/Easy Build System/Tools/Quick Setup...";
        public const int PRIORITY = 1;
        public const string VALIDATE_ACTION_KEY = "Buildings/Build Inputs/Build Input - Validate Action";
        public const string CANCEL_ACTION_KEY = "Buildings/Build Inputs/Build Input - Cancel Action";
        public const string ROTATE_ACTION_KEY = "Buildings/Build Inputs/Build Input - Rotate Action";
        public const string SELECT_ACTION_KEY = "Buildings/Build Inputs/Build Input - Select Action";
        public const string PLACEMENT_ACTION_KEY = "Buildings/Build Inputs/Build Input - Placement Action";
        public const string DESTRUCTION_ACTION_KEY = "Buildings/Build Inputs/Build Input - Destruction Action";
        public const string ADJUSTMENT_ACTION_KEY = "Buildings/Build Inputs/Build Input - Adjustment Action";
        public const string DEFAULT_VIEW = "FirstPerson";

        public static readonly string[] CUSTOM_PARTS_PATH =
        {
            "Building Parts/Building_Cube",
            "Building Parts/Building_Cylinder"
        };

        public string MenuPath => MENU_PATH;
        public int Priority => PRIORITY;

        public bool Validate()
        {
            if (BuildingManager.Instance)
            {
                Debug.LogWarning("Quick Setup already done.");
                return false;
            }

            if (Camera.main == null)
            {
                Debug.LogWarning("Quick Setup requires a Main Camera (tag 'MainCamera').");
                return false;
            }

            return true;
        }

        public void Execute()
        {
            GameObject managerGameObject = new GameObject("Building Manager");
            Undo.RegisterCreatedObjectUndo(managerGameObject, "Create Building Manager");
            Undo.AddComponent<BuildingManager>(managerGameObject);

            Camera mainCameraComponent = Camera.main;

            BuildingController buildingControllerComponent = mainCameraComponent.GetComponent<BuildingController>();
            if (buildingControllerComponent == null)
            {
                buildingControllerComponent = Undo.AddComponent<BuildingController>(mainCameraComponent.gameObject);
            }

            BuildingInput buildingInputComponent = mainCameraComponent.GetComponent<BuildingInput>();
            if (buildingInputComponent == null)
            {
                buildingInputComponent = Undo.AddComponent<BuildingInput>(mainCameraComponent.gameObject);
            }
            buildingInputComponent.hideFlags = HideFlags.HideInInspector;
            buildingInputComponent.Reset();

            SerializedObject inputSerializedObject = new SerializedObject(buildingInputComponent);
            inputSerializedObject.FindProperty("m_useCustomPartsSelection").boolValue = true;

            List<string> customPartIdsList = new List<string>(CUSTOM_PARTS_PATH.Length);

            for (int i = 0; i < CUSTOM_PARTS_PATH.Length; i++)
            {
                string buildingPartResourcePath = CUSTOM_PARTS_PATH[i];
                BuildingPart loadedBuildingPart = Resources.Load<BuildingPart>(buildingPartResourcePath);

                if (loadedBuildingPart == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(loadedBuildingPart.PrefabId))
                {
                    continue;
                }

                customPartIdsList.Add(loadedBuildingPart.PrefabId);
            }

            SerializedProperty customPartsArrayProperty = inputSerializedObject.FindProperty("m_customPartReferences");
            customPartsArrayProperty.arraySize = customPartIdsList.Count;

            for (int i = 0; i < customPartIdsList.Count; i++)
            {
                customPartsArrayProperty.GetArrayElementAtIndex(i).stringValue = customPartIdsList[i];
            }

            inputSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            FirstPersonBuildingView firstPersonViewComponent = buildingControllerComponent.GetComponent<FirstPersonBuildingView>();
            if (firstPersonViewComponent == null)
            {
                firstPersonViewComponent = Undo.AddComponent<FirstPersonBuildingView>(buildingControllerComponent.gameObject);
                firstPersonViewComponent.OriginTransform = mainCameraComponent.transform;
            }
            firstPersonViewComponent.hideFlags = HideFlags.HideInInspector;

            Debug.Log("Quick Setup completed successfully!");
        }
    }
}