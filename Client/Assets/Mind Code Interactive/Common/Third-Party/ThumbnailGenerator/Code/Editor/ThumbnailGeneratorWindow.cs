/// <summary>
/// Project : Thumbnail Generator
/// Class : ThumbnailGeneratorWindow.cs
/// Namespace : MindCodeInteractive.ThumbnailGenerator.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace MindCodeInteractive.ThumbnailGenerator.Code.Editor
{
    public class ThumbnailGeneratorWindow : EditorWindow
    {
        private const string k_BottomTrademark = "Copyright © 2015-2026 Mind Code Interactive.";
        private const string k_WindowTitle = "Thumbnail Generator";

        private enum Resolution
        {
            _128x128,
            _256x256,
            _512x512,
            _1024x1024,
            _2048x2048,
            _4096x4096
        }

        private static GameObject s_targetModel;
        private static Texture2D s_previewTexture;
        private static Action<Texture2D> s_onTextureGenerated;
        private static Resolution s_outputResolution = Resolution._512x512;
        private static double s_refreshInterval = 0.5f;
        private static double s_nextRefreshAt;
        private static string s_lastExportPath = "";

        private static ThumbnailSettings SettingsRef => ThumbnailGenerator.Settings;

        private Texture2D TransparentCheckerTexture => EditorGUIUtility.isProSkin
            ? EditorGUIUtility.LoadRequired("Previews/Textures/textureCheckerDark.png") as Texture2D
            : EditorGUIUtility.LoadRequired("Previews/Textures/textureChecker.png") as Texture2D;

        public static void ShowWindow(GameObject model, Action<Texture2D> onTextureGeneratedCallback)
        {
            ThumbnailGeneratorWindow window = GetWindow<ThumbnailGeneratorWindow>(k_WindowTitle);
            window.minSize = new Vector2(600f, 560f);
            window.titleContent = new GUIContent(k_WindowTitle);

            s_targetModel = model;
            s_onTextureGenerated = onTextureGeneratedCallback;

            ResetPreview();
            Initialize();
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += EditorTick;

            if (SettingsRef != null)
            {
                LoadSettings();
            }
        }

        private void OnDisable()
        {
            if (SettingsRef != null)
            {
                SaveSettings();
                EditorUtility.SetDirty(SettingsRef);
                AssetDatabase.SaveAssets();
            }
            Cleanup();

            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.update -= EditorTick;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPreview();
            EditorGUILayout.EndHorizontal();
        }

        private static void EditorTick()
        {
            if (EditorApplication.timeSinceStartup < s_nextRefreshAt || s_targetModel == null)
            {
                return;
            }

            s_nextRefreshAt = EditorApplication.timeSinceStartup + s_refreshInterval;
            RenderPreview();
        }

        private void OnPlayModeChanged(PlayModeStateChange _)
        {
            Cleanup();
            Close();
        }

        private static void Initialize()
        {
            ThumbnailGenerator.Initialize();
            LoadSettings();
            RenderPreview();
        }

        private static void ResetPreview()
        {
            Cleanup();
            ThumbnailPreviewHandler.Reset();
            s_nextRefreshAt = 0;
        }

        private static void Cleanup()
        {
            s_previewTexture = null;
            ThumbnailGenerator.ClearPostProcessing();
            ThumbnailGenerator.Cleanup();
        }

        private void DrawLeftPanel()
        {
            GUIStyle container = new GUIStyle { padding = new RectOffset(7, 7, 7, 7) };
            EditorGUILayout.BeginVertical(container, GUILayout.Width(300f));

            GUILayout.Label(k_WindowTitle, new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 });
            EditorGUILayout.Separator();
            DrawModelField();
            EditorGUILayout.Separator();
            DrawRenderSettings();
            EditorGUILayout.Separator();
            DrawCameraSettings();
            EditorGUILayout.Separator();
            DrawLightingSettings();
            EditorGUILayout.Separator();
            DrawAmbientSettings();
            EditorGUILayout.Separator();
            DrawExportSettings();

            GUILayout.FlexibleSpace();
            GUILayout.Label(k_BottomTrademark, new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9
            });

            EditorGUILayout.EndVertical();
        }

        private void DrawModelField()
        {
            EditorGUI.BeginChangeCheck();
            GameObject newModel = (GameObject)EditorGUILayout.ObjectField("Model", s_targetModel, typeof(GameObject), false);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Change Target Model");
                s_targetModel = newModel;

                if (s_targetModel != null)
                {
                    RenderPreview();
                }
                else
                {
                    Cleanup();
                }
            }
        }

        private void DrawRenderSettings()
        {
            GUILayout.Label("Render Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            Color bg = EditorGUILayout.ColorField("Background", SettingsRef.BackgroundColor);
            ThumbnailSettings.AntiAliasingMode aa = (ThumbnailSettings.AntiAliasingMode)EditorGUILayout.EnumPopup("Anti-Aliasing", SettingsRef.AntiAliasing);

            DrawPipelineProfileFields();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SettingsRef, "Change Render");
                SettingsRef.BackgroundColor = bg;
                SettingsRef.AntiAliasing = aa;

                if (ThumbnailGenerator.CachedCamera != null)
                {
                    ThumbnailGenerator.CachedCamera.backgroundColor = bg;
                }

                EditorUtility.SetDirty(SettingsRef);
                RenderPreview();
            }
        }

        private void DrawPipelineProfileFields()
        {
            RenderPipelineType pipeline = RenderPipelineDetector.Current;

            if (pipeline == RenderPipelineType.HDRP || pipeline == RenderPipelineType.URP)
            {
                Type volumeProfileType = ReflectionHelper.FindType("UnityEngine.Rendering.VolumeProfile");
                if (volumeProfileType == null)
                {
                    return;
                }

                ScriptableObject vp = (ScriptableObject)EditorGUILayout.ObjectField(
                    "Volume Profile", SettingsRef.VolumeProfileRef, volumeProfileType, false);

                if (vp != SettingsRef.VolumeProfileRef)
                {
                    SettingsRef.VolumeProfileRef = vp;
                    ThumbnailGenerator.ApplyVolumeProfile(vp);
                }
            }
            else if (RenderPipelineDetector.HasPostProcessingStackV2)
            {
                Type ppType = ReflectionHelper.FindType("UnityEngine.Rendering.PostProcessing.PostProcessProfile");
                if (ppType == null)
                {
                    return;
                }

                ScriptableObject pp = (ScriptableObject)EditorGUILayout.ObjectField(
                    "Post Process Profile", SettingsRef.PostProcessProfileRef, ppType, false);

                if (pp != SettingsRef.PostProcessProfileRef)
                {
                    SettingsRef.PostProcessProfileRef = pp;
                    ThumbnailGenerator.ApplyPostProcessProfile(pp);
                }
            }
        }

        private void DrawCameraSettings()
        {
            GUILayout.Label("Camera Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            bool ortho = EditorGUILayout.Toggle("Orthographic", SettingsRef.OrthographicView);
            float dist = EditorGUILayout.Slider("Distance", SettingsRef.OrbitDistance, 0.5f, 10f);
            Vector3 position = EditorGUILayout.Vector3Field("Position", SettingsRef.Position);
            float pitch = EditorGUILayout.Slider("Pitch", SettingsRef.OrbitAngles.x, -89f, 89f);
            float yaw = EditorGUILayout.Slider("Yaw", SettingsRef.OrbitAngles.y, -180f, 180f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SettingsRef, "Change Camera");
                SettingsRef.OrbitDistance = dist;
                SettingsRef.OrthographicView = ortho;
                SettingsRef.Position = position;
                SettingsRef.OrbitAngles = new Vector2(pitch, yaw);
                RenderPreview();
            }

            if (GUILayout.Button("Auto Focus (3/4)", GUILayout.Height(22)))
            {
                AutoFocusCamera();
            }
        }

        private static void AutoFocusCamera()
        {
            if (s_targetModel == null || ThumbnailGenerator.CachedCamera == null)
            {
                return;
            }

            if (!TryGetRenderableBounds(s_targetModel, out _))
            {
                return;
            }

            Undo.RecordObject(SettingsRef, "Auto Focus Camera");
            SettingsRef.OrbitAngles = new Vector2(22f, 135f);
            SettingsRef.Position = Vector3.zero;
            SettingsRef.OrbitDistance = 1.25f;
            RenderPreview();
        }

        private void DrawLightingSettings()
        {
            GUILayout.Label("Lighting Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Direction", GUILayout.Width(80));
            float lightYaw = EditorGUILayout.Slider(SettingsRef.LightRotation.y, 0f, 360f);
            SettingsRef.LightRotation = new Vector3(SettingsRef.LightRotation.x, lightYaw, 0f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height", GUILayout.Width(80));
            float lightPitch = EditorGUILayout.Slider(SettingsRef.LightRotation.x, 0f, 180f);
            SettingsRef.LightRotation = new Vector3(lightPitch, SettingsRef.LightRotation.y, 0f);
            EditorGUILayout.EndHorizontal();

            float lightIntensity = EditorGUILayout.Slider("Intensity", SettingsRef.LightIntensity, 0f, 10f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SettingsRef, "Change Light");
                SettingsRef.LightIntensity = lightIntensity;
                RenderPreview();
            }
        }

        private void DrawAmbientSettings()
        {
            GUILayout.Label("Ambient Settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            AmbientMode newAm = (AmbientMode)EditorGUILayout.EnumPopup("Ambient Mode", SettingsRef.AmbientMode);
            Color newAc = EditorGUILayout.ColorField("Ambient Color", SettingsRef.AmbientColor);
            float newAi = SettingsRef.AmbientIntensity;

            if (newAm != AmbientMode.Trilight && newAm != AmbientMode.Flat)
            {
                newAi = EditorGUILayout.Slider("Ambient Intensity", SettingsRef.AmbientIntensity, 0f, 2f);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(SettingsRef, "Change Ambient");
                SettingsRef.AmbientMode = newAm;
                SettingsRef.AmbientColor = newAc;
                SettingsRef.AmbientIntensity = newAi;
                RenderPreview();
            }
        }

        private void DrawExportSettings()
        {
            GUILayout.Label("Export Settings", EditorStyles.boldLabel);

            Resolution newRes = (Resolution)EditorGUILayout.EnumPopup("Resolution", s_outputResolution);
            if (newRes != s_outputResolution)
            {
                s_outputResolution = newRes;
                RenderPreview();
            }

            EditorGUILayout.LabelField("Last Export Path:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(s_lastExportPath, EditorStyles.miniLabel);

            GUI.enabled = s_targetModel != null;
            if (GUILayout.Button("Export", GUILayout.Height(24)))
            {
                GenerateThumbnail();
            }

            GUI.enabled = true;
        }

        private void DrawRightPreview()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            Rect previewRect = GUILayoutUtility.GetRect(100f, 100f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            Color border = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.5f, 0.5f, 0.5f);
            EditorGUI.DrawRect(previewRect, border);

            Rect inner = new Rect(previewRect.x + 1f, previewRect.y + 1f, previewRect.width - 2f, previewRect.height - 2f);

            if (s_targetModel != null && SettingsRef != null)
            {
                ThumbnailPreviewHandler.HandleInteraction(inner, SettingsRef, this, out bool needsRender);
                if (needsRender)
                {
                    RenderPreview();
                }
            }

            if (s_previewTexture != null)
            {
                ThumbnailPreviewHandler.DrawTexture(inner, s_previewTexture, SettingsRef, TransparentCheckerTexture);
            }
            else
            {
                ThumbnailPreviewHandler.DrawPlaceholder(inner);
                GameObject dropped = ThumbnailPreviewHandler.HandleDragAndDrop(inner);
                if (dropped != null)
                {
                    s_targetModel = dropped;
                    RenderPreview();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void GenerateThumbnail()
        {
            ThumbnailGenerator.Settings = SettingsRef;
            Vector2Int resolution = GetResolution();

            if (s_targetModel != null)
            {
                s_previewTexture = ThumbnailGenerator.CreateModelPreview(s_targetModel, resolution.x, resolution.y);
            }

            if (s_previewTexture == null)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Thumbnail",
                s_targetModel != null ? s_targetModel.name : "thumbnail",
                "png",
                "Please enter a file name to save the thumbnail to");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            Texture2D exportTex = ThumbnailPreviewHandler.EnsureReadable(s_previewTexture);
            byte[] bytes = exportTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            DestroyImmediate(exportTex);

            s_lastExportPath = path;
            SaveSettings();
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.alphaIsTransparency = SettingsRef.BackgroundColor.a < 1f;
                importer.SaveAndReimport();
            }

            Texture2D createdTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            s_onTextureGenerated?.Invoke(createdTexture);
        }

        private static void RenderPreview()
        {
            ThumbnailGenerator.Settings = SettingsRef;
            if (s_targetModel == null)
            {
                return;
            }

            Vector2Int resolution = GetResolution();
            s_previewTexture = ThumbnailGenerator.CreateModelPreview(s_targetModel, resolution.x, resolution.y);
        }

        private static bool TryGetRenderableBounds(GameObject root, out Bounds bounds)
        {
            bounds = default;
            if (root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer r = renderers[i];
                if (r == null || !r.enabled || r is ParticleSystemRenderer)
                {
                    continue;
                }

                if (!hasBounds) { bounds = r.bounds; hasBounds = true; }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }

            return hasBounds;
        }

        private static Vector2Int GetResolution()
        {
            switch (s_outputResolution)
            {
                case Resolution._128x128: return new Vector2Int(128, 128);
                case Resolution._256x256: return new Vector2Int(256, 256);
                case Resolution._512x512: return new Vector2Int(512, 512);
                case Resolution._1024x1024: return new Vector2Int(1024, 1024);
                case Resolution._2048x2048: return new Vector2Int(2048, 2048);
                case Resolution._4096x4096: return new Vector2Int(4096, 4096);
                default: return new Vector2Int(512, 512);
            }
        }

        private static void SaveSettings()
        {
            ThumbnailSettingsSerializer.Save(SettingsRef, (int)s_outputResolution, s_lastExportPath);
        }

        private static void LoadSettings()
        {
            ThumbnailSettingsSerializer.Load(SettingsRef, out int res, out string path);
            s_outputResolution = (Resolution)res;
            s_lastExportPath = path;
        }
    }
}