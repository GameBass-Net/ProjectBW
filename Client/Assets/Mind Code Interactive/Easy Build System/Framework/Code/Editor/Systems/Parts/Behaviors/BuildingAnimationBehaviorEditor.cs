/// <summary>
/// Project : Easy Build System
/// Class : BuildingAnimationBehaviorEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Behaviors
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors;
using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Behaviors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingAnimationBehavior))]
    public class BuildingAnimationBehaviorEditor : BaseInspectorEditor<BuildingAnimationBehavior>
    {
        protected override void OnInspectorDraw()
        {
            SerializedProperty mappingsArray = Properties.Get("m_eventMappings");

            if (mappingsArray.arraySize == 0)
            {
                EditorGUILayout.Space(1f);
                EditorGUIExtended.Label("No animation mappings added yet.", EditorGUILabels.LabelType.Mini, EditorGUILabels.LabelAlignment.Center);
            }

            for (int mappingIndex = 0; mappingIndex < mappingsArray.arraySize; mappingIndex++)
            {
                DrawAnimationMapping(mappingIndex, mappingsArray);
            }

            EditorGUILayout.Separator();

            if (GUILayout.Button("Add Animation Mapping...", EditorStyles.miniButton))
            {
                AddAnimationMapping(mappingsArray);
            }

            if (AnimatorStatePlayerDrawer.IsPlaying)
            {
                Repaint();
            }
        }

        private void DrawAnimationMapping(int mappingIndex, SerializedProperty mappingsArray)
        {
            if (mappingsArray == null || mappingIndex < 0 || mappingIndex >= mappingsArray.arraySize)
            {
                return;
            }

            SerializedProperty mappingElement = mappingsArray.GetArrayElementAtIndex(mappingIndex);
            if (mappingElement == null)
            {
                return;
            }

            SerializedProperty eventTypeProperty = mappingElement.FindPropertyRelative("EventType");
            if (eventTypeProperty == null)
            {
                return;
            }

            string eventTypeName = (eventTypeProperty.enumValueIndex >= 0 && eventTypeProperty.enumValueIndex < eventTypeProperty.enumDisplayNames.Length)
                ? eventTypeProperty.enumDisplayNames[eventTypeProperty.enumValueIndex]
                : "Unknown";
            string sectionTitle = $"Event {mappingIndex + 1} - {eventTypeName}";
            string propertyPathPrefix = $"m_eventMappings.{mappingIndex}";

            EditorGUIExtended.ExpandableSectionWithPane(
                new GUIContent(sectionTitle, "Animation settings for this event."),
                string.Empty,
                () =>
                {
                    Properties.Draw($"{propertyPathPrefix}.EventType", new GUIContent("Trigger", "When to play animation."));
                    Properties.Draw($"{propertyPathPrefix}.Type", new GUIContent("Type", "Animator or Procedural."));

                    SerializedProperty typeProperty = Properties.Get($"{propertyPathPrefix}.Type");
                    int animationTypeIndex = typeProperty?.enumValueIndex ?? 0;

                    if ((BuildingAnimationBehavior.AnimationType)animationTypeIndex == BuildingAnimationBehavior.AnimationType.Animator)
                    {
                        Properties.Draw($"{propertyPathPrefix}.m_animator", new GUIContent("Animator", "Target Animator component."));
                        Properties.Draw($"{propertyPathPrefix}.m_animatorSpeed", new GUIContent("Speed", "Playback speed multiplier."));
                        Properties.Draw($"{propertyPathPrefix}.m_animatorLayerIndex", new GUIContent("Layer Index", "Animator layer index."));
                        Properties.Draw($"{propertyPathPrefix}.m_animatorStateName", new GUIContent("State Name", "Animator state to play."));
                    }
                    else
                    {
                        Properties.Draw($"{propertyPathPrefix}.m_duration", new GUIContent("Duration", "Animation duration in seconds."));
                        Properties.Draw($"{propertyPathPrefix}.m_scaleCurve", new GUIContent("Scale Curve", "Scale animation over time."));
                        Properties.Draw($"{propertyPathPrefix}.m_rotationCurve", new GUIContent("Rotation Curve", "Rotation animation over time."));
                        Properties.Draw($"{propertyPathPrefix}.m_positionCurve", new GUIContent("Position Curve", "Position animation over time."));
                    }
                },
                menu => DrawMappingContextMenu(menu, mappingIndex, mappingsArray));
        }

        private void DrawMappingContextMenu(GenericMenu contextMenuToPopulate, int mappingIndex, SerializedProperty mappingsArray)
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

        private void AddAnimationMapping(SerializedProperty mappingsArray)
        {
            mappingsArray.arraySize++;
            SerializedProperty newMappingElement = mappingsArray.GetArrayElementAtIndex(mappingsArray.arraySize - 1);

            newMappingElement.FindPropertyRelative("EventType").enumValueIndex = 0;
            newMappingElement.FindPropertyRelative("Type").enumValueIndex = 0;
            newMappingElement.FindPropertyRelative("m_animator").objectReferenceValue = null;
            newMappingElement.FindPropertyRelative("m_animatorSpeed").floatValue = 1f;
            newMappingElement.FindPropertyRelative("m_animatorLayerIndex").intValue = 0;
            newMappingElement.FindPropertyRelative("m_animatorStateName").stringValue = string.Empty;
            newMappingElement.FindPropertyRelative("m_duration").floatValue = 0.5f;
            newMappingElement.FindPropertyRelative("m_scaleCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 1, 1, 1);
            newMappingElement.FindPropertyRelative("m_rotationCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 0);
            newMappingElement.FindPropertyRelative("m_positionCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 0);
        }
    }
}