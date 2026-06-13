/// <summary>
/// Project : Mind Code Interactive
/// Class : AudioClipPlayerEditor.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.AudioSystem
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.AudioSystem;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.AudioSystem
{
    [CustomPropertyDrawer(typeof(AudioClipPlayer))]
    public class AudioClipPlayerEditor : PropertyDrawer
    {
        private enum PreviewAnchor { None, Camera, Selection }

        private static GameObject s_previewGameObject;
        private static double s_stopTime;

        private const float BUTTON_SPACING = 15f;
        private const float VERTICAL_PADDING = 3f;
        private const float MIN_PITCH = -3f;
        private const float MAX_PITCH = 3f;
        private const float DOPPLER_LEVEL = 0f;
        private const float SPREAD = 0f;
        private const float MIN_DISTANCE_CLAMP = 0.01f;
        private const float MIN_DISTANCE_BUFFER = 0.01f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = lineHeight + verticalSpacing;

            if (!property.isExpanded)
            {
                return totalHeight;
            }

            SerializedProperty selectProperty = property.FindPropertyRelative("m_select");
            SerializedProperty clipsProperty = property.FindPropertyRelative("m_clips");
            SerializedProperty modeProperty = property.FindPropertyRelative("m_mode");

            totalHeight += lineHeight + verticalSpacing;

            if ((AudioClipPlayer.SelectMode)selectProperty.enumValueIndex == AudioClipPlayer.SelectMode.Single)
            {
                totalHeight += lineHeight + verticalSpacing;
            }
            else
            {
                totalHeight += EditorGUI.GetPropertyHeight(clipsProperty, true) + verticalSpacing;
            }

            totalHeight += (lineHeight + verticalSpacing) * 6;

            if ((AudioClipPlayer.PlayMode)modeProperty.enumValueIndex == AudioClipPlayer.PlayMode.ThreeD)
            {
                totalHeight += (lineHeight + verticalSpacing) * 4;
            }

            return totalHeight + 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
            Rect headerRect = new Rect(position.x, position.y, position.width, lineHeight);
            Rect foldoutRect = new Rect(headerRect.x, headerRect.y, headerRect.width - 120f, lineHeight);
            Rect buttonRect = new Rect(headerRect.xMax - 135f, headerRect.y, 150f, lineHeight);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            DrawPlayButton(buttonRect, property);

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;

            Rect fieldRect = new Rect(position.x, headerRect.yMax + verticalSpacing, position.width, lineHeight);

            SerializedProperty selectProperty = property.FindPropertyRelative("m_select");
            SerializedProperty clipProperty = property.FindPropertyRelative("m_clip");
            SerializedProperty clipsProperty = property.FindPropertyRelative("m_clips");
            SerializedProperty mixerProperty = property.FindPropertyRelative("m_mixer");
            SerializedProperty modeProperty = property.FindPropertyRelative("m_mode");
            SerializedProperty volumeProperty = property.FindPropertyRelative("m_volume");
            SerializedProperty volumeJitterProperty = property.FindPropertyRelative("m_volumeJitter");
            SerializedProperty pitchProperty = property.FindPropertyRelative("m_pitch");
            SerializedProperty pitchJitterProperty = property.FindPropertyRelative("m_pitchJitter");
            SerializedProperty spatialBlendProperty = property.FindPropertyRelative("m_spatialBlend");
            SerializedProperty rolloffProperty = property.FindPropertyRelative("m_rolloff");
            SerializedProperty minDistanceProperty = property.FindPropertyRelative("m_minDistance");
            SerializedProperty maxDistanceProperty = property.FindPropertyRelative("m_maxDistance");

            EditorGUI.PropertyField(fieldRect, selectProperty);
            fieldRect.y += lineHeight + verticalSpacing;

            if ((AudioClipPlayer.SelectMode)selectProperty.enumValueIndex == AudioClipPlayer.SelectMode.Single)
            {
                EditorGUI.PropertyField(fieldRect, clipProperty);
                fieldRect.y += lineHeight + verticalSpacing;
            }
            else
            {
                float arrayHeight = EditorGUI.GetPropertyHeight(clipsProperty, true);
                Rect arrayRect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width, arrayHeight);
                EditorGUI.PropertyField(arrayRect, clipsProperty, true);
                fieldRect.y += arrayHeight + verticalSpacing;
            }

            EditorGUI.PropertyField(fieldRect, mixerProperty);
            fieldRect.y += lineHeight + verticalSpacing;
            EditorGUI.PropertyField(fieldRect, modeProperty);
            fieldRect.y += lineHeight + verticalSpacing;
            EditorGUI.PropertyField(fieldRect, volumeProperty);
            fieldRect.y += lineHeight + verticalSpacing;
            EditorGUI.PropertyField(fieldRect, volumeJitterProperty);
            fieldRect.y += lineHeight + verticalSpacing;
            EditorGUI.PropertyField(fieldRect, pitchProperty);
            fieldRect.y += lineHeight + verticalSpacing;
            EditorGUI.PropertyField(fieldRect, pitchJitterProperty);
            fieldRect.y += lineHeight + verticalSpacing;

            if ((AudioClipPlayer.PlayMode)modeProperty.enumValueIndex == AudioClipPlayer.PlayMode.ThreeD)
            {
                EditorGUI.PropertyField(fieldRect, spatialBlendProperty);
                fieldRect.y += lineHeight + verticalSpacing;
                EditorGUI.PropertyField(fieldRect, rolloffProperty);
                fieldRect.y += lineHeight + verticalSpacing;
                EditorGUI.PropertyField(fieldRect, minDistanceProperty);
                fieldRect.y += lineHeight + verticalSpacing;
                EditorGUI.PropertyField(fieldRect, maxDistanceProperty);
                fieldRect.y += lineHeight + verticalSpacing;
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        private void DrawPlayButton(Rect rect, SerializedProperty property)
        {
            rect = EditorGUI.IndentedRect(rect);
            float buttonWidth = (rect.width - BUTTON_SPACING) / 3f;
            Rect playButtonRect = new Rect(rect.x + 2f * buttonWidth, rect.y, buttonWidth, rect.height);

            GUIContent playIcon = GetIcon("d_PlayButton", "Play");
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageOnly,
                padding = new RectOffset(0, 0, (int)VERTICAL_PADDING, (int)VERTICAL_PADDING)
            };

            using (new EditorGUI.DisabledScope(GetPreviewClip(property) == null))
            {
                if (GUI.Button(playButtonRect, playIcon, buttonStyle))
                {
                    PreviewAudio(property, PreviewAnchor.None);
                }
            }
        }

        private void StopPreview()
        {
            if (s_previewGameObject == null)
            {
                return;
            }

            Object.DestroyImmediate(s_previewGameObject);
            s_previewGameObject = null;
            EditorApplication.update -= OnPreviewTick;
        }

        private void OnPreviewTick()
        {
            if (EditorApplication.timeSinceStartup >= s_stopTime)
            {
                StopPreview();
            }
        }

        private void PreviewAudio(SerializedProperty property, PreviewAnchor anchor)
        {
            AudioClip audioClip = GetRandomPreviewClip(property);
            if (audioClip == null)
            {
                StopPreview();
                return;
            }

            float volume = property.FindPropertyRelative("m_volume")?.floatValue ?? 1f;
            float volumeJitter = property.FindPropertyRelative("m_volumeJitter")?.floatValue ?? 0f;
            float pitch = property.FindPropertyRelative("m_pitch")?.floatValue ?? 1f;
            float pitchJitter = property.FindPropertyRelative("m_pitchJitter")?.floatValue ?? 0f;
            AudioMixerGroup mixer = property.FindPropertyRelative("m_mixer")?.objectReferenceValue as AudioMixerGroup;
            AudioClipPlayer.PlayMode playMode = (AudioClipPlayer.PlayMode)property.FindPropertyRelative("m_mode").enumValueIndex;
            float spatialBlend = property.FindPropertyRelative("m_spatialBlend")?.floatValue ?? 0f;
            AudioRolloffMode rolloffMode = (AudioRolloffMode)property.FindPropertyRelative("m_rolloff").enumValueIndex;
            float minDistance = property.FindPropertyRelative("m_minDistance")?.floatValue ?? 1f;
            float maxDistance = property.FindPropertyRelative("m_maxDistance")?.floatValue ?? 500f;

            float randomizedVolume = Mathf.Clamp01(volume + Random.Range(-volumeJitter, volumeJitter));
            float randomizedPitch = Mathf.Clamp(pitch + Random.Range(-pitchJitter, pitchJitter), MIN_PITCH, MAX_PITCH);

            if (s_previewGameObject != null)
            {
                AudioSource previousSource = s_previewGameObject.GetComponent<AudioSource>();
                if (previousSource != null && previousSource.isPlaying && previousSource.clip == audioClip)
                {
                    StopPreview();
                    return;
                }
                StopPreview();
            }

            s_previewGameObject = EditorUtility.CreateGameObjectWithHideFlags("__EditorAudioPreview__", HideFlags.HideAndDontSave, typeof(AudioSource));
            AudioSource audioSource = s_previewGameObject.GetComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.outputAudioMixerGroup = mixer;
            audioSource.volume = randomizedVolume;
            audioSource.pitch = randomizedPitch;
            audioSource.loop = false;
            audioSource.dopplerLevel = DOPPLER_LEVEL;
            audioSource.spread = SPREAD;

            if (playMode == AudioClipPlayer.PlayMode.ThreeD)
            {
                audioSource.spatialBlend = Mathf.Clamp01(spatialBlend);
                audioSource.rolloffMode = rolloffMode;
                audioSource.minDistance = Mathf.Max(MIN_DISTANCE_CLAMP, minDistance);
                audioSource.maxDistance = Mathf.Max(audioSource.minDistance + MIN_DISTANCE_BUFFER, maxDistance);
            }
            else
            {
                audioSource.spatialBlend = 0f;
            }

            if (anchor == PreviewAnchor.Camera && SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                s_previewGameObject.transform.position = SceneView.lastActiveSceneView.camera.transform.position;
            }
            else if (anchor == PreviewAnchor.Selection && Selection.activeTransform != null)
            {
                s_previewGameObject.transform.position = Selection.activeTransform.position;
            }
            else
            {
                s_previewGameObject.transform.position = Vector3.zero;
            }

            audioSource.Stop();
            audioSource.Play();
            s_stopTime = EditorApplication.timeSinceStartup + (audioClip.length / Mathf.Max(0.0001f, Mathf.Abs(randomizedPitch)));
            EditorApplication.update -= OnPreviewTick;
            EditorApplication.update += OnPreviewTick;
        }

        private AudioClip GetPreviewClip(SerializedProperty property)
        {
            SerializedProperty selectProperty = property.FindPropertyRelative("m_select");
            SerializedProperty clipProperty = property.FindPropertyRelative("m_clip");
            SerializedProperty clipsProperty = property.FindPropertyRelative("m_clips");

            if ((AudioClipPlayer.SelectMode)selectProperty.enumValueIndex == AudioClipPlayer.SelectMode.Single)
            {
                return clipProperty.objectReferenceValue as AudioClip;
            }

            if (clipsProperty != null && clipsProperty.isArray)
            {
                for (int i = 0; i < clipsProperty.arraySize; i++)
                {
                    SerializedProperty elementProperty = clipsProperty.GetArrayElementAtIndex(i);
                    AudioClip clip = elementProperty.objectReferenceValue as AudioClip;
                    if (clip != null)
                    {
                        return clip;
                    }
                }
            }

            return clipProperty.objectReferenceValue as AudioClip;
        }

        private AudioClip GetRandomPreviewClip(SerializedProperty property)
        {
            AudioClipPlayer.SelectMode selectMode = (AudioClipPlayer.SelectMode)property.FindPropertyRelative("m_select").enumValueIndex;
            if (selectMode == AudioClipPlayer.SelectMode.Single)
            {
                return property.FindPropertyRelative("m_clip").objectReferenceValue as AudioClip;
            }

            SerializedProperty clipsProperty = property.FindPropertyRelative("m_clips");
            if (clipsProperty == null || !clipsProperty.isArray || clipsProperty.arraySize == 0)
            {
                return null;
            }

            int arraySize = clipsProperty.arraySize;
            int attemptCount = arraySize;

            while (attemptCount-- > 0)
            {
                int randomIndex = Mathf.FloorToInt(Random.value * arraySize);
                SerializedProperty elementProperty = clipsProperty.GetArrayElementAtIndex(randomIndex);
                AudioClip clip = elementProperty.objectReferenceValue as AudioClip;
                if (clip != null)
                {
                    return clip;
                }
            }

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty elementProperty = clipsProperty.GetArrayElementAtIndex(i);
                AudioClip clip = elementProperty.objectReferenceValue as AudioClip;
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private GUIContent GetIcon(string iconName, string fallbackText)
        {
            GUIContent icon = EditorGUIUtility.IconContent(iconName);
            return icon != null && icon.image != null ? icon : new GUIContent(fallbackText);
        }
    }
}