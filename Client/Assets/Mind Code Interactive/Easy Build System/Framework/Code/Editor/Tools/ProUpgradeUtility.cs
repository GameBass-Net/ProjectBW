/// <summary>
/// Project : Pro Build System
/// Class : ProUpgradeUtility.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;

#if PRO_BUILD_SYSTEM
using MindCodeInteractive.ProBuildSystem.Framework.Code.Runtime.Systems.Controllers;
using MindCodeInteractive.ProBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.ProBuildSystem.Framework.Code.Runtime.Systems.Controllers.States.Implementations;
using MindCodeInteractive.ProBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations;
using MindCodeInteractive.ProBuildSystem.Framework.Code.Runtime.Systems.Managers;
using MindCodeInteractive.ProBuildSystem.Framework.Code.Runtime.Systems.Parts;
#endif

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools
{
    public static class ProUpgradeUtility
    {
        private static readonly Dictionary<Type, Type> k_viewMap = new Dictionary<Type, Type>
        {
#if PRO_BUILD_SYSTEM
            { typeof(FirstPersonBuildingView),  typeof(FirstPersonProBuildingView) },
            { typeof(ThirdPersonBuildingView),  typeof(ThirdPersonProBuildingView) },
            { typeof(TopDownBuildingView),      typeof(TopDownProBuildingView) },
            { typeof(OrbitalBuildingView),      typeof(OrbitalProBuildingView) },
#endif
        };

        private static readonly Dictionary<Type, Type> k_stateMap = new Dictionary<Type, Type>
        {
#if PRO_BUILD_SYSTEM
            { typeof(PlacementBuildingState),   typeof(ProPlacementBuildingState) },
            { typeof(AdjustmentBuildingState),  typeof(ProAdjustmentBuildingState) },
            { typeof(DestructionBuildingState), typeof(ProDestructionBuildingState) },
            { typeof(UpgradeBuildingState), typeof(ProUpgradeBuildingState) }
#endif
        };

        public static void DrawManagerUpgradeBanner(BuildingManager manager)
        {
#if PRO_BUILD_SYSTEM
            if (manager is BuildingProManager)
            {
                return;
            }

            EditorGUILayout.HelpBox("Pro Build System is available. Upgrade this manager to enable Pro features across all components in the scene � the Building Manager is the core of the system.", MessageType.Info);

            if (!GUILayout.Button("Upgrade to Pro Build System Manager..."))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "Upgrade to Pro Manager",
                "This will replace the BuildingManager with a BuildingProManager.\n\nAll current settings will be preserved.\n\nAre you sure?",
                "Upgrade", "Cancel"))
            {
                return;
            }

            UpgradeManager(manager);
#endif
        }

        public static void DrawControllerUpgradeBanner(BuildingController controller)
        {
#if PRO_BUILD_SYSTEM
            if (controller is BuildingProController)
            {
                return;
            }

            EditorGUILayout.HelpBox("Pro Build System is available. Upgrade this controller to enable Pro features across all components in the scene � the Building Controller is the core of the system.", MessageType.Info);

            if (!GUILayout.Button("Upgrade to Pro Build System Controller..."))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "Upgrade to Pro Controller",
                "This will replace the BuildingController with a BuildingProController and swap all views and states to their Pro equivalents.\n\nAll current settings will be preserved.\n\nAre you sure?",
                "Upgrade", "Cancel"))
            {
                return;
            }

            UpgradeController(controller);
#endif
        }

        public static void DrawSocketUpgradeBanner(BuildingSocket socket)
        {
#if PRO_BUILD_SYSTEM
            if (socket is BuildingProSocket)
            {
                return;
            }

            EditorGUILayout.HelpBox("Pro Build System is available. Upgrade this socket to enable socket constraint features.", MessageType.Info);

            if (!GUILayout.Button("Upgrade to Pro Build System Socket..."))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "Upgrade to Pro Socket",
                "This will replace the BuildingSocket with a BuildingProSocket.\n\nAll current settings will be preserved.\n\nAre you sure?",
                "Upgrade", "Cancel"))
            {
                return;
            }

            UpgradeSocket(socket);
#endif
        }

        public static void DrawPartUpgradeBanner(BuildingPart part)
        {
#if PRO_BUILD_SYSTEM
            if (part is BuildingProPart)
            {
                return;
            }

            EditorGUILayout.HelpBox("Pro Build System is available. Upgrade this part to enable Pro features such as outline highlighting in the editor placer.", MessageType.Info);

            if (!GUILayout.Button("Upgrade to Pro Build System Part..."))
            {
                return;
            }

            if (!EditorUtility.DisplayDialog(
                "Upgrade to Pro Part",
                "This will replace the BuildingPart with a BuildingProPart.\n\nAll current settings will be preserved.\n\nAre you sure?",
                "Upgrade", "Cancel"))
            {
                return;
            }

            UpgradePart(part);
#endif
        }

#if PRO_BUILD_SYSTEM
        private static void UpgradePart(BuildingPart part)
        {
            GameObject go = part.gameObject;
            string partJson = JsonUtility.ToJson(part);
            Undo.DestroyObjectImmediate(part);
            BuildingProPart proPart = Undo.AddComponent<BuildingProPart>(go);
            JsonUtility.FromJsonOverwrite(partJson, proPart);
            EditorUtility.SetDirty(go);
            GUIUtility.ExitGUI();
        }

        private static void UpgradeSocket(BuildingSocket socket)
        {
            GameObject go = socket.gameObject;
            string socketJson = JsonUtility.ToJson(socket);
            Undo.DestroyObjectImmediate(socket);
            BuildingProSocket proSocket = Undo.AddComponent<BuildingProSocket>(go);
            JsonUtility.FromJsonOverwrite(socketJson, proSocket);
            EditorUtility.SetDirty(go);
            GUIUtility.ExitGUI();
        }

        private static void UpgradeManager(BuildingManager manager)
        {
            GameObject go = manager.gameObject;

            string groupingJson = JsonUtility.ToJson(manager.GroupingSettings);
            string batchingJson = JsonUtility.ToJson(manager.BatchingSettings);
            string saveJson = JsonUtility.ToJson(manager.SaveSettings);
            string gridJson = JsonUtility.ToJson(manager.GridSettings);
            string physicsJson = JsonUtility.ToJson(manager.PhysicsSettings);
            LayerMask socketLayer = manager.SocketLayer;
            BuildingManager.DetectionType detectionType = manager.SocketDetectionType;

            Undo.DestroyObjectImmediate(manager);

            BuildingProManager proManager = Undo.AddComponent<BuildingProManager>(go);

            JsonUtility.FromJsonOverwrite(groupingJson, proManager.GroupingSettings);
            JsonUtility.FromJsonOverwrite(batchingJson, proManager.BatchingSettings);
            JsonUtility.FromJsonOverwrite(saveJson, proManager.SaveSettings);
            JsonUtility.FromJsonOverwrite(gridJson, proManager.GridSettings);
            JsonUtility.FromJsonOverwrite(physicsJson, proManager.PhysicsSettings);
            proManager.SocketLayer = socketLayer;
            proManager.SocketDetectionType = detectionType;

            EditorUtility.SetDirty(go);
            GUIUtility.ExitGUI();
        }

        private static void UpgradeController(BuildingController controller)
        {
            GameObject go = controller.gameObject;

            BuildingView[] existingViews = controller.Views;
            BuildingState[] existingStates = controller.States;

            Undo.DestroyObjectImmediate(controller);

            BuildingProController proController = Undo.AddComponent<BuildingProController>(go);

            List<BuildingView> proViews = new List<BuildingView>();

            if (existingViews != null)
            {
                foreach (BuildingView view in existingViews)
                {
                    if (view == null)
                    {
                        continue;
                    }

                    string viewJson = JsonUtility.ToJson(view);
                    Type proViewType = k_viewMap.TryGetValue(view.GetType(), out Type mappedView) ? mappedView : view.GetType();

                    Undo.DestroyObjectImmediate(view);

                    BuildingView proView = Undo.AddComponent(go, proViewType) as BuildingView;
                    if (proView != null)
                    {
                        JsonUtility.FromJsonOverwrite(viewJson, proView);
                        proView.hideFlags = HideFlags.HideInInspector;
                        proViews.Add(proView);
                    }
                }
            }

            List<BuildingState> proStates = new List<BuildingState>();

            if (existingStates != null)
            {
                foreach (BuildingState state in existingStates)
                {
                    if (state == null)
                    {
                        continue;
                    }

                    string stateJson = JsonUtility.ToJson(state);
                    Type proStateType = k_stateMap.TryGetValue(state.GetType(), out Type mappedState) ? mappedState : state.GetType();

                    Undo.DestroyObjectImmediate(state);

                    BuildingState proState = Undo.AddComponent(go, proStateType) as BuildingState;
                    if (proState != null)
                    {
                        JsonUtility.FromJsonOverwrite(stateJson, proState);
                        proState.hideFlags = HideFlags.HideInInspector;
                        proStates.Add(proState);
                    }
                }
            }

            proController.Views = proViews.ToArray();
            proController.States = proStates.ToArray();

            EditorUtility.SetDirty(go);
            GUIUtility.ExitGUI();
        }
#endif
    }
}