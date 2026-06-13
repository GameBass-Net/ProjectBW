/// <summary>
/// Project : Mind Code Interactive
/// Class : AnimatorStatePlayerDrawer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Drawers
{
    [CustomPropertyDrawer(typeof(AnimatorStateAttribute))]
    public class AnimatorStatePlayerDrawer : PropertyDrawer
    {
        private static bool s_isPlaying;
        private static string s_activeKey;
        private static Animator s_playingAnimator;
        private static Animator s_targetAnimator;
        private static AnimationClip s_playingClip;
        private static AnimationClip s_currentClip;
        private static float s_playingTime;
        private static float s_playingSpeed;
        private static float s_previewTime;
        private static float s_speed = 1f;
        private static double s_lastEditorTime;
        private static bool s_isSceneGuiSubscribed;

        public static bool IsPlaying
        {
            get => s_isPlaying;
            set => s_isPlaying = value;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            AnimatorStateAttribute attribute = (AnimatorStateAttribute)this.attribute;

            if (!string.IsNullOrEmpty(attribute.SourceFieldName))
            {
                SerializedProperty sourceProperty = GetSiblingProperty(property, attribute.SourceFieldName);
                Animator targetAnimator = sourceProperty?.objectReferenceValue as Animator;

                if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
                {
                    return EditorGUIUtility.singleLineHeight;
                }
            }

            return EditorGUIUtility.singleLineHeight * 2f + 20f + 2f * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Selection.activeGameObject != s_targetAnimator?.gameObject && s_isPlaying)
            {
                StopPlayback();
                if (s_targetAnimator != null)
                {
                    s_targetAnimator.Rebind();
                }
            }

            if (!s_isSceneGuiSubscribed)
            {
                SceneView.duringSceneGui += OnSceneGUI;
                Selection.selectionChanged += OnSelectionChanged;
                s_isSceneGuiSubscribed = true;
            }

            EditorGUI.BeginProperty(position, label, property);

            AnimatorStateAttribute attribute = (AnimatorStateAttribute)this.attribute;
#pragma warning disable CS0618
            string key = property.serializedObject.targetObject.GetInstanceID() + "|" + property.propertyPath;
#pragma warning restore CS0618
            bool isActive = key == s_activeKey;

            s_speed = attribute.DefaultSpeed;
            string speedFieldName = string.IsNullOrEmpty(attribute.SpeedFieldName) ? "AnimatorSpeed" : attribute.SpeedFieldName;
            SerializedProperty speedProperty = GetSiblingProperty(property, speedFieldName);
            if (speedProperty?.propertyType == SerializedPropertyType.Float)
            {
                s_speed = speedProperty.floatValue;
            }

            s_targetAnimator = null;
            if (!string.IsNullOrEmpty(attribute.SourceFieldName))
            {
                SerializedProperty sourceProperty = GetSiblingProperty(property, attribute.SourceFieldName);
                s_targetAnimator = sourceProperty?.objectReferenceValue as Animator;
            }

            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect popupRect = new Rect(position.x, position.y, position.width, lineHeight);

            if (s_targetAnimator == null)
            {
                GUI.enabled = false;
                EditorGUI.Popup(popupRect, label.text, 0, new string[] { "No Animator assigned" });
                GUI.enabled = true;
                EditorGUI.EndProperty();
                return;
            }

            AnimatorController controller = s_targetAnimator.runtimeAnimatorController as AnimatorController;
            if (controller == null)
            {
                GUI.enabled = false;
                EditorGUI.Popup(popupRect, label.text, 0, new string[] { "No Controller assigned" });
                GUI.enabled = true;
                EditorGUI.EndProperty();
                return;
            }

            string layerFieldName = string.IsNullOrEmpty(attribute.LayerFieldName) ? "AnimatorLayerIndex" : attribute.LayerFieldName;
            SerializedProperty layerProperty = GetSiblingProperty(property, layerFieldName);
            int layerIndex = 0;
            if (layerProperty?.propertyType == SerializedPropertyType.Integer)
            {
                layerIndex = Mathf.Clamp(layerProperty.intValue, 0, controller.layers.Length - 1);
            }

            List<string> stateNames = new List<string>();
            foreach (ChildAnimatorState childState in controller.layers[layerIndex].stateMachine.states)
            {
                stateNames.Add(childState.state.name);
            }

            int currentIndex = string.IsNullOrEmpty(property.stringValue) ? 0 : stateNames.IndexOf(property.stringValue);
            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            int newIndex = EditorGUI.Popup(popupRect, label.text, currentIndex, stateNames.ToArray());
            string selectedStateName = stateNames.Count > 0 ? stateNames[newIndex] : "";

            if (property.stringValue != selectedStateName)
            {
                property.stringValue = selectedStateName;
                property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                s_isPlaying = false;
                s_previewTime = 0f;
            }

            s_currentClip = null;
            if (newIndex >= 0)
            {
                string selected = stateNames[newIndex];
                foreach (AnimatorControllerLayer layer in controller.layers)
                {
                    s_currentClip = FindClipInStateMachine(layer.stateMachine, selected);
                    if (s_currentClip != null)
                    {
                        break;
                    }
                }
            }

            Rect indentedRect = EditorGUI.IndentedRect(position);
            Rect timelineRect = new Rect(indentedRect.x, position.y + lineHeight + 2f, indentedRect.width, 20f);
            DrawTimeline(timelineRect);

            float controlsY = position.y + lineHeight + 2f + 20f + 2f;
            float buttonCount = 5f;
            float totalSpacing = 10f * (buttonCount - 1f);
            float buttonsWidth = lineHeight * buttonCount + totalSpacing;
            float labelWidth = (position.width - buttonsWidth - 10f * 2f) / 2f;

            Rect leftLabelRect = new Rect(position.x + 5f, controlsY, labelWidth, lineHeight);
            Rect rightLabelRect = new Rect(position.x + position.width - labelWidth - 5f, controlsY, labelWidth, lineHeight);

            float buttonX = position.x + labelWidth + 10f;
            Event currentEvent = Event.current;

            if (s_currentClip != null)
            {
                GUI.color = Color.white / 1.25f;
                GUIStyle leftStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, fontSize = 10 };
                EditorGUI.LabelField(leftLabelRect, "Clip: " + s_currentClip.name, leftStyle);
                GUI.color = Color.white;

                string[] icons = {
                    "d_Animation.FirstKey",
                    "d_Animation.PrevKey",
                    (isActive && s_isPlaying) ? "d_PauseButton" : "d_Animation.Play",
                    "d_Animation.NextKey",
                    "d_Animation.LastKey"
                };

                Action[] actions = {
                    () => HandleFirstFrame(isActive),
                    () => HandlePreviousFrame(isActive),
                    () => HandlePlayPause(key, isActive),
                    () => HandleNextFrame(isActive),
                    () => HandleLastFrame(isActive)
                };

                for (int i = 0; i < 5; i++)
                {
                    Rect buttonRect = new Rect(buttonX, controlsY, lineHeight, lineHeight);
                    GUIContent buttonContent = EditorGUIUtility.IconContent(icons[i], "");
                    if (GUI.Button(buttonRect, buttonContent, EditorStyles.iconButton))
                    {
                        actions[i].Invoke();
                        currentEvent.Use();
                    }
                    buttonX += lineHeight + 10f;
                }

                GUIStyle rightStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight, fontSize = 10 };
                string timeDisplay = s_currentClip.length > 0 ? (isActive ? s_playingTime : s_previewTime).ToString("F2") + "s / " + s_currentClip.length.ToString("F2") + "s" : "0s";
                EditorGUI.LabelField(rightLabelRect, timeDisplay, rightStyle);
            }
            else
            {
                EditorGUI.LabelField(leftLabelRect, "No animation clip found");
            }

            EditorGUI.EndProperty();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!s_isPlaying || s_playingAnimator == null || s_playingClip == null)
            {
                return;
            }

            double currentEditorTime = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentEditorTime - s_lastEditorTime);
            s_lastEditorTime = currentEditorTime;

            s_playingTime += deltaTime * s_playingSpeed;
            if (s_playingTime >= s_playingClip.length)
            {
                s_playingTime = 0f;
            }

            SamplePlaying();
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeGameObject != s_playingAnimator?.gameObject)
            {
                StopPlayback();
            }
        }

        private void HandleFirstFrame(bool isActive)
        {
            if (isActive)
            {
                s_playingTime = 0f;
                SamplePlaying();
            }
            else
            {
                s_previewTime = 0f;
                SamplePreview();
            }
        }

        private void HandlePreviousFrame(bool isActive)
        {
            if (isActive)
            {
                s_playingTime = Mathf.Max(0f, s_playingTime - 1f / s_playingClip.frameRate);
                s_isPlaying = false;
                SamplePlaying();
            }
            else
            {
                s_previewTime = Mathf.Max(0f, s_previewTime - 1f / s_currentClip.frameRate);
                SamplePreview();
            }
        }

        private void HandlePlayPause(string key, bool isActive)
        {
            if (isActive && s_isPlaying)
            {
                StopPlayback();
            }
            else
            {
                s_activeKey = key;
                s_playingAnimator = s_targetAnimator;
                s_playingClip = s_currentClip;
                s_playingTime = 0f;
                s_playingSpeed = s_speed;
                s_lastEditorTime = EditorApplication.timeSinceStartup;
                s_isPlaying = true;

                if (!AnimationMode.InAnimationMode())
                {
                    AnimationMode.StartAnimationMode();
                }

                if (s_playingClip != null && s_playingAnimator != null)
                {
                    AnimationMode.BeginSampling();
                    s_playingClip.SampleAnimation(s_playingAnimator.gameObject, 0f);
                    AnimationMode.EndSampling();
                }
            }
        }

        private void HandleNextFrame(bool isActive)
        {
            if (isActive)
            {
                s_playingTime = Mathf.Min(s_playingClip.length, s_playingTime + 1f / s_playingClip.frameRate);
                s_isPlaying = false;
                SamplePlaying();
            }
            else
            {
                s_previewTime = Mathf.Min(s_currentClip.length, s_previewTime + 1f / s_currentClip.frameRate);
                SamplePreview();
            }
        }

        private void HandleLastFrame(bool isActive)
        {
            if (isActive)
            {
                s_playingTime = s_playingClip.length;
                s_isPlaying = false;
                SamplePlaying();
            }
            else
            {
                s_previewTime = s_currentClip.length;
                SamplePreview();
            }
        }

        private static void SamplePreview()
        {
            if (s_currentClip != null && s_targetAnimator != null)
            {
                if (!AnimationMode.InAnimationMode())
                {
                    AnimationMode.StartAnimationMode();
                }

                AnimationMode.BeginSampling();
                s_currentClip.SampleAnimation(s_targetAnimator.gameObject, s_previewTime);
                AnimationMode.EndSampling();
                EditorUtility.SetDirty(s_targetAnimator.gameObject);
                SceneView.RepaintAll();
            }
        }

        private static void SamplePlaying()
        {
            if (s_playingClip != null && s_playingAnimator != null)
            {
                if (!AnimationMode.InAnimationMode())
                {
                    AnimationMode.StartAnimationMode();
                }

                AnimationMode.BeginSampling();
                s_playingClip.SampleAnimation(s_playingAnimator.gameObject, s_playingTime);
                AnimationMode.EndSampling();
                EditorUtility.SetDirty(s_playingAnimator.gameObject);
                SceneView.RepaintAll();
            }
        }

        private static void DrawTimeline(Rect timelineRect)
        {
            bool isActiveClip = s_currentClip == s_playingClip && s_targetAnimator == s_playingAnimator;
            EditorGUI.DrawRect(timelineRect, new Color(0f, 0f, 0f, 0.25f));

            float length = s_currentClip?.length ?? 1f;
            float currentTime = isActiveClip ? s_playingTime : s_previewTime;
            float timelineX = timelineRect.x + (currentTime / length) * timelineRect.width;

            Handles.color = Color.yellow;
            Handles.DrawLine(new Vector2(timelineX, timelineRect.y), new Vector2(timelineX, timelineRect.y + timelineRect.height));
            Handles.color = Color.gray * 0.5f;

            float interval = length / 10;
            for (int i = 0; i <= 10; i++)
            {
                float tickTime = i * interval;
                float tickX = timelineRect.x + (tickTime / length) * timelineRect.width;
                Handles.DrawLine(new Vector2(tickX, timelineRect.y), new Vector2(tickX, timelineRect.y + timelineRect.height));
            }
        }

        private static SerializedProperty GetSiblingProperty(SerializedProperty property, string siblingName)
        {
            string propertyPath = property.propertyPath;
            int lastDotIndex = propertyPath.LastIndexOf('.');
            string pathPrefix = lastDotIndex >= 0 ? propertyPath.Substring(0, lastDotIndex + 1) : "";
            string siblingPath = pathPrefix + siblingName;
            return property.serializedObject.FindProperty(siblingPath);
        }

        private static AnimationClip FindClipInStateMachine(AnimatorStateMachine stateMachine, string stateName)
        {
            foreach (ChildAnimatorState childState in stateMachine.states)
            {
                if (childState.state.name == stateName && childState.state.motion != null)
                {
                    AnimationClip clip = ExtractAnimationClip(childState.state.motion);
                    if (clip != null)
                    {
                        return clip;
                    }
                }
            }

            foreach (ChildAnimatorStateMachine subStateMachine in stateMachine.stateMachines)
            {
                AnimationClip clip = FindClipInStateMachine(subStateMachine.stateMachine, stateName);
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private static AnimationClip ExtractAnimationClip(Motion motion)
        {
            if (motion is AnimationClip clip)
            {
                return clip;
            }

            if (motion is BlendTree blendTree)
            {
                foreach (ChildMotion childMotion in blendTree.children)
                {
                    if (childMotion.motion is AnimationClip childClip)
                    {
                        return childClip;
                    }
                }
            }

            return null;
        }

        private static void StopPlayback()
        {
            s_isPlaying = false;
            s_activeKey = null;

            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }
        }
    }
}