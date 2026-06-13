/// <summary>
/// Project : Easy Build System
/// Class : BuildingDebrisBehaviorEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Behaviors
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Behaviors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingDebrisBehavior))]
    public class BuildingDebrisBehaviorEditor : BaseInspectorEditor<BuildingDebrisBehavior>
    {
        protected override void OnInspectorDraw()
        {
            EditorGUIExtended.Separator("Trigger Settings", false);
            Properties.Draw("m_trigger", new GUIContent("Trigger", "Event that triggers debris spawning."));

            SerializedProperty triggerProperty = Properties.Get("m_trigger");
            if (triggerProperty?.enumValueIndex == (int)BuildingDebrisBehavior.DestructionTrigger.OnFallingImpact)
            {
                using (EditorGUIExtended.IndentScope())
                {
                    Properties.Draw("m_impactSpeedThreshold", new GUIContent("Impact Speed Threshold", "Minimum impact velocity to trigger debris."));
                }
            }

            EditorGUIExtended.Separator("Debris Mappings");
            SerializedProperty variantSpawnsArray = Properties.Get("m_variantSpawns");

            if (variantSpawnsArray.arraySize == 0)
            {
                EditorGUILayout.Space(1f);
                EditorGUIExtended.Label("No debris mappings added yet.", EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
            }

            for (int variantIndex = 0; variantIndex < variantSpawnsArray.arraySize; variantIndex++)
            {
                DrawDebrisMapping(variantIndex, variantSpawnsArray);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Add Debris Mapping...", EditorStyles.miniButton))
            {
                variantSpawnsArray.arraySize++;
            }

            EditorGUIExtended.Separator("Physics Settings");
            Properties.Draw("m_upwardForce", new GUIContent("Upward Force", "Upward impulse applied to debris."));
            Properties.Draw("m_minForce", new GUIContent("Min Force", "Minimum random force magnitude."));
            Properties.Draw("m_maxForce", new GUIContent("Max Force", "Maximum random force magnitude."));
            Properties.Draw("m_randomTorque", new GUIContent("Random Torque", "Random angular impulse applied to debris."));
            Properties.Draw("m_maxDepenetrationVelocity", new GUIContent("Depenetration Limit", "Prevents physics explosions from overlap."));

            EditorGUIExtended.Separator("Despawn Settings");
            Properties.Draw("m_lifetime", new GUIContent("Lifetime", "Destroy debris after N seconds. 0 = never despawn."));
            Properties.Draw("m_ignoreLayer", new GUIContent("Ignore Collision Layer", "Physics layer for debris collision filtering."));
        }

        private void DrawDebrisMapping(int mappingIndex, SerializedProperty mappingsArray)
        {
            if (mappingsArray == null || mappingIndex < 0 || mappingIndex >= mappingsArray.arraySize)
            {
                return;
            }

            string variantDisplayName = GetVariantDisplayName(mappingIndex);
            string sectionTitle = $"Variant {mappingIndex + 1} - {variantDisplayName}";
            string propertyPathPrefix = $"m_variantSpawns.{mappingIndex}";

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent(sectionTitle, "Debris settings for this variant."),
                string.Empty,
                () =>
                {
                    Properties.Draw($"{propertyPathPrefix}.m_prefab", new GUIContent("Prefab", "Debris prefab to instantiate."));

                    using (EditorGUIExtended.IndentScope())
                    {
                        Properties.Draw($"{propertyPathPrefix}.m_sound", new GUIContent("Sound", "Audio clip played when debris spawns."));
                    }

                    Properties.Draw($"{propertyPathPrefix}.m_positionOffset", new GUIContent("Position Offset", "Local position offset for debris spawn."));
                    Properties.Draw($"{propertyPathPrefix}.m_rotationOffset", new GUIContent("Rotation Offset", "Local rotation offset for debris spawn."));
                },
                menu => DrawDebrisMappingContextMenu(menu, mappingIndex, mappingsArray));
        }

        private void DrawDebrisMappingContextMenu(GenericMenu contextMenuToPopulate, int mappingIndex, SerializedProperty mappingsArray)
        {
            bool canMoveUp = mappingIndex > 0;
            bool canMoveDown = mappingIndex < mappingsArray.arraySize - 1;

            EditorContextMenus.AddMoveUpItem(contextMenuToPopulate, canMoveUp, () =>
            {
                serializedObject.Update();
                mappingsArray.MoveArrayElement(mappingIndex, mappingIndex - 1);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            });

            EditorContextMenus.AddMoveDownItem(contextMenuToPopulate, canMoveDown, () =>
            {
                serializedObject.Update();
                mappingsArray.MoveArrayElement(mappingIndex, mappingIndex + 1);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            });

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddDuplicateItem(contextMenuToPopulate, () =>
            {
                serializedObject.Update();
                mappingsArray.InsertArrayElementAtIndex(mappingIndex);
                serializedObject.ApplyModifiedProperties();
                Repaint();
            });

            EditorContextMenus.Separator(contextMenuToPopulate);

            EditorContextMenus.AddRemoveComponentItem(contextMenuToPopulate, Target, () =>
            {
                serializedObject.Update();
                mappingsArray.DeleteArrayElementAtIndex(mappingIndex);
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                Repaint();
            });
        }

        private string GetVariantDisplayName(int variantIndex)
        {
            BuildingPart part = Target?.GetComponent<BuildingPart>();
            BuildingRendererSystem rendererSystem = part?.RendererSystem;

            if (rendererSystem?.Variants == null || variantIndex < 0 || variantIndex >= rendererSystem.Variants.Count)
            {
                return $"Variant {variantIndex}";
            }

            RendererVariantData rendererVariantData = rendererSystem.Variants[variantIndex];
            if (rendererVariantData == null)
            {
                return $"Variant {variantIndex}";
            }

            if (!string.IsNullOrEmpty(rendererVariantData.Name))
            {
                return rendererVariantData.Name;
            }

            if (rendererVariantData.Root)
            {
                return rendererVariantData.Root.name;
            }

            return $"Variant {variantIndex}";
        }
    }
}