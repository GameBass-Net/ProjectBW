/// <summary>
/// Project : Mind Code Interactive
/// Class : BuildingPartEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.ThumbnailGenerator.Code.Editor;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Tools;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingPart))]
    public class BuildingPartEditor : BaseInspectorEditor<BuildingPart>
    {
        private BuildingPart.BuildingState m_originalState = BuildingPart.BuildingState.None;
        private bool m_isPreviewingInEditor;
        private readonly Dictionary<int, Texture2D> m_previewCache = new Dictionary<int, Texture2D>();
        private List<string> m_integrityMessages = new List<string>();

        protected override void OnInspectorEnable()
        {
            Target.OnEnable();
            RegisterChildrenListAccessor(p => p.ConditionSystem.GetAllConditions());
            RegisterChildrenListAccessor(p => p.BehaviorSystem.GetAllBehaviors());
            m_originalState = Target.State;
            m_isPreviewingInEditor = false;
            ValidateSetup();
        }

        protected override void OnInspectorDisable()
        {
            Target.PlacementSystem.ClearDirectionIndicator();
            if (m_isPreviewingInEditor && Target)
            {
                try { Target.PlacementSystem?.RestoreMaterials(); } catch { }
                try { Target.SetState(m_originalState); } catch { }
                m_isPreviewingInEditor = false;
            }
            m_previewCache.Clear();
        }

        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.InspectorHeader(target,
                "Defines a part in the scene with its own data, placement, behaviors, conditions and renderer settings.\n" +
                "Manages renderer variants that allow visual switching without redefining existing settings.\n" +
                "Presets allow configuration to be shared and reused across multiple Building Parts.\n" +
                "See the documentation for more information about this component.");

#if PRO_BUILD_SYSTEM
            ProUpgradeUtility.DrawPartUpgradeBanner(Target);
#endif

            DrawGeneralSection();
            DrawRenderersSection();
            DrawPlacementSection();
            DrawBehaviorsSection();
            DrawConditionsSection();
            DrawDebugSection();

            EditorGUIExtended.InspectorBottom();
        }

        private void ValidateSetup()
        {
            if (Target == null)
            {
                return;
            }

            foreach (BuildingBehavior behavior in Target.BehaviorSystem.GetAllBehaviors())
            {
                if (behavior != null)
                {
                    behavior.hideFlags = HideFlags.HideInInspector;
                }
            }

            foreach (BuildingCondition condition in Target.ConditionSystem.GetAllConditions())
            {
                condition.hideFlags = HideFlags.HideInInspector;
            }

            BuildingPartRenderersEditor.CheckIntegrity(Target, m_integrityMessages);
        }

        private void DrawGeneralSection()
        {
            EditorGUIExtended.DrawExpandableSection("General Settings", "general",
                "Configure the Building Part identity, category, thumbnail and manage presets.",
                () =>
                {
                    BuildingPartPresetLinkEditor.Draw(Target, Properties, serializedObject);

                    using (EditorGUIExtended.DisabledScope(true))
                    {
                        bool isPrefabPacked = PrefabUtility.IsPartOfPrefabAsset(Target.gameObject) ||
                                             (PrefabUtility.IsPartOfPrefabInstance(Target.gameObject) &&
                                              !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(Target.gameObject))));

                        if (isPrefabPacked)
                        {
                            Properties.Draw("m_prefabId", new GUIContent("Building Prefab ID", "Unique identifier for this Building Part prefab."));
                        }
                        else
                        {
                            EditorGUILayout.TextField(new GUIContent("Building Prefab ID", "Must be stored in asset."), "Must be stored in asset...");
                        }

                        bool isInValidScene = Target.gameObject.scene.IsValid() && !string.IsNullOrEmpty(Target.gameObject.scene.name);

                        if (isInValidScene)
                        {
                            Properties.Draw("m_uniqueId", new GUIContent("Building Unique ID", "Instance identifier for this Building Part in the scene."));
                        }
                        else
                        {
                            EditorGUILayout.TextField(new GUIContent("Building Unique ID", "Must be in a scene."), "Must be in a scene...");
                        }
                    }

                    SerializedProperty nameProperty = serializedObject.FindProperty("name");
                    string shownName = nameProperty != null && !string.IsNullOrEmpty(nameProperty.stringValue)
                        ? nameProperty.stringValue
                        : Target.gameObject.name;

                    EditorGUI.BeginChangeCheck();
                    string newName = EditorGUILayout.TextField(new GUIContent("Building Name", "Display name shown in UI and the inspector."), shownName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(Target, "Rename Building Part");
                        Undo.RecordObject(Target.gameObject, "Rename GameObject");
                        if (nameProperty != null)
                        {
                            nameProperty.stringValue = newName;
                        }

                        serializedObject.ApplyModifiedProperties();
                        Target.gameObject.name = newName;
                        EditorUtility.SetDirty(Target);
                        EditorUtility.SetDirty(Target.gameObject);
                    }

                    Properties.Draw("m_description", new GUIContent("Building Description", "Short description displayed in UI for this Building Part."));

                    EditorGUI.BeginChangeCheck();
                    Properties.Draw("m_category", new GUIContent("Building Category", "Category this Building Part belongs to."));
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorApplication.delayCall += () => BuildingPartRegistry.Instance.RefreshRegistry();
                        return;
                    }

                    Properties.Draw("m_thumbnail", new GUIContent("Building Thumbnail", "Icon displayed when browsing Building Parts in UI."));

                    if (GUILayout.Button("Generate Thumbnail..."))
                    {
                        ThumbnailGeneratorWindow.ShowWindow(Target.gameObject, generatedThumbnail =>
                        {
                            if (generatedThumbnail != null)
                            {
                                Undo.RecordObject(Target, "Set Thumbnail");
                                SerializedObject serializedObjectForThumbnail = new SerializedObject(Target);
                                SerializedProperty thumbnailProp = serializedObjectForThumbnail.FindProperty("m_thumbnail");
                                thumbnailProp.objectReferenceValue = generatedThumbnail;
                                serializedObjectForThumbnail.ApplyModifiedProperties();
                                EditorUtility.SetDirty(Target);
                                AssetDatabase.SaveAssets();
                                Debug.Log("Thumbnail generated for building part '" + Target.Name + "'.");
                            }
                            else
                            {
                                Debug.LogError("Failed to generate thumbnail.");
                            }
                        });
                    }
                },
                false,
                true);
        }

        #region Renderers

        private void DrawRenderersSection()
        {
            Target.RendererSystem.DrawGizmos = EditorGUIExtended.DrawExpandableSection("Renderers Settings", "frame",
                "Configure renderer variants and their material sets for each building state.",
                () => BuildingPartRenderersEditor.Draw(Target, Properties, serializedObject, GetOrCreateEditor, m_integrityMessages),
                false,
                true);
        }

        #endregion

        #region Behaviors

        private void DrawBehaviorsSection()
        {
            EditorGUIExtended.DrawExpandableSection("Behaviors Settings", "gear_play",
                "Configure behaviors attached to this Building Part such as animations and debris on destruction.",
                () => BuildingPartBehaviorsEditor.Draw(Target, TargetsTyped, GetOrCreateChildMultiEditor),
                false,
                true);
        }

        #endregion

        #region Conditions

        private void DrawConditionsSection()
        {
            bool isOpen = EditorGUIExtended.DrawExpandableSection(
                "Conditions Settings",
                "tree",
                "Configure placement conditions and validation rules that must be satisfied to place this building part.",
                () => BuildingPartConditionsEditor.Draw(Target, TargetsTyped, GetOrCreateChildMultiEditor),
                false,
                true);

            if (!isOpen)
            {
                BuildingPartConditionsEditor.DisableGizmos(Target);
            }
        }

        #endregion

        #region Placement

        private void DrawPlacementSection()
        {
            Target.PlacementSystem.DrawGizmos = EditorGUIExtended.DrawExpandableSection("Placement Settings", "preview",
                "Configure preview materials, positioning, rotation, grid snapping and direction indicators.",
                () =>
                {
                    BuildingPartPlacementEditor.Draw(Target, Properties, serializedObject, ref m_originalState, ref m_isPreviewingInEditor);
                    DrawPlacementExtras();
                },
                false,
                true);
        }

        protected virtual void DrawPlacementExtras() { }

        #endregion

        #region Debug

        private void DrawDebugSection()
        {
            EditorGUIExtended.DrawExpandableSection("Debug Settings", "cache",
                "Inspect the current Building Part state, statistics and configure debug rendering options.",
                () =>
                {
                    EditorGUIExtended.Separator("Unique Object Statistics", false);
                    EditorGUILayout.LabelField("Unique ID :", Target.UniqueId.ToString());
                    EditorGUILayout.LabelField("Is Registered :", Target.IsRegistered.ToString());

                    EditorGUIExtended.Separator("Building Part Statistics");
                    EditorGUILayout.LabelField("Current State :", Target.State.ToString());
                    EditorGUILayout.LabelField("Last State :", Target.LastState.ToString());
                    EditorGUILayout.LabelField("Is Preview :", Target.IsPreview.ToString());
                    EditorGUILayout.LabelField("Is Placed :", Target.IsPlaced.ToString());
                    EditorGUILayout.LabelField("Is Runtime Instantiated :", Target.IsRuntimeInstantiated.ToString());
                    EditorGUILayout.LabelField("Is Saveable :", Target.IsSaveable.ToString());
                    EditorGUILayout.LabelField("Attached Socket :", Target.AttachedSocket != null ? Target.AttachedSocket.name : "None");
                    EditorGUILayout.LabelField("Attached Group :", Target.AttachedGroup != null ? Target.AttachedGroup.name : "None");
                    EditorGUILayout.LabelField("Attached Collection :", Target.AttachedCollection != null ? Target.AttachedCollection.name : "None");

                    EditorGUIExtended.Separator("Rendering Settings");
                    Properties.Draw("m_debugFlags",
                        new GUIContent("Debug Draw Flags", "Controls where Building Part boundaries are rendered in the editor."));
                },
                false,
                false);
        }

        #endregion

        [InitializeOnLoad]
        static class BuildingPartInitializer
        {
            static BuildingPartInitializer()
            {
                ObjectFactory.componentWasAdded += OnComponentAdded;
            }

            private static void OnComponentAdded(Component component)
            {
                if (component is BuildingPart bp)
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (bp == null || Application.isPlaying)
                        {
                            return;
                        }

                        Debug.LogWarning($"Building Parts must be created from the creation menu (Tools/Mind Code Interactive/Easy Build System/Components).", bp);
                        UnityEngine.Object.DestroyImmediate(component, true);
                    };
                }
            }
        }
    }
}