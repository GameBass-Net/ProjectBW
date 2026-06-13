/// <summary>
/// Project : Thumbnail Generator
/// Class : ThumbnailSettingsSerializer.cs
/// Namespace : MindCodeInteractive.ThumbnailGenerator.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace MindCodeInteractive.ThumbnailGenerator.Code.Editor
{
    public static class ThumbnailSettingsSerializer
    {
        private const string k_Prefix = "ThumbnailGenerator_";

        public static void Save(ThumbnailSettings settings, int outputResolution, string lastExportPath)
        {
            if (settings == null)
            {
                Debug.LogWarning("ThumbnailSettings is null, cannot save");
                return;
            }

            SaveCamera(settings);
            SaveLight(settings);
            SaveAmbient(settings);
            SaveRender(settings);
            SaveExport(outputResolution, lastExportPath);
            SavePipeline(settings);
        }

        public static void Load(ThumbnailSettings settings, out int outputResolution, out string lastExportPath)
        {
            outputResolution = 2;
            lastExportPath = "";

            if (settings == null)
            {
                return;
            }

            LoadCamera(settings);
            LoadLight(settings);
            LoadAmbient(settings);
            LoadRender(settings);
            LoadExport(out outputResolution, out lastExportPath);
            LoadPipeline(settings);
        }

        private static void SaveCamera(ThumbnailSettings s)
        {
            EditorPrefs.SetFloat(k_Prefix + "OrbitDistance", s.OrbitDistance);
            EditorPrefs.SetFloat(k_Prefix + "OrbitPitch", s.OrbitAngles.x);
            EditorPrefs.SetFloat(k_Prefix + "OrbitYaw", s.OrbitAngles.y);
            EditorPrefs.SetString(k_Prefix + "PositionX", s.Position.x.ToString());
            EditorPrefs.SetString(k_Prefix + "PositionY", s.Position.y.ToString());
            EditorPrefs.SetString(k_Prefix + "PositionZ", s.Position.z.ToString());
            EditorPrefs.SetInt(k_Prefix + "OrthographicView", s.OrthographicView ? 1 : 0);
        }

        private static void SaveLight(ThumbnailSettings s)
        {
            EditorPrefs.SetFloat(k_Prefix + "LightYaw", s.LightRotation.y);
            EditorPrefs.SetFloat(k_Prefix + "LightPitch", s.LightRotation.x);
            EditorPrefs.SetFloat(k_Prefix + "LightIntensity", s.LightIntensity);
        }

        private static void SaveAmbient(ThumbnailSettings s)
        {
            EditorPrefs.SetInt(k_Prefix + "AmbientMode", (int)s.AmbientMode);
            EditorPrefs.SetString(k_Prefix + "AmbientColorR", s.AmbientColor.r.ToString());
            EditorPrefs.SetString(k_Prefix + "AmbientColorG", s.AmbientColor.g.ToString());
            EditorPrefs.SetString(k_Prefix + "AmbientColorB", s.AmbientColor.b.ToString());
            EditorPrefs.SetString(k_Prefix + "AmbientColorA", s.AmbientColor.a.ToString());
            EditorPrefs.SetFloat(k_Prefix + "AmbientIntensity", s.AmbientIntensity);
        }

        private static void SaveRender(ThumbnailSettings s)
        {
            EditorPrefs.SetString(k_Prefix + "BgColorR", s.BackgroundColor.r.ToString());
            EditorPrefs.SetString(k_Prefix + "BgColorG", s.BackgroundColor.g.ToString());
            EditorPrefs.SetString(k_Prefix + "BgColorB", s.BackgroundColor.b.ToString());
            EditorPrefs.SetString(k_Prefix + "BgColorA", s.BackgroundColor.a.ToString());
            EditorPrefs.SetInt(k_Prefix + "AntiAliasing", (int)s.AntiAliasing);
        }

        private static void SaveExport(int outputResolution, string lastExportPath)
        {
            EditorPrefs.SetInt(k_Prefix + "OutputResolution", outputResolution);
            EditorPrefs.SetString(k_Prefix + "LastExportPath", lastExportPath);
        }

        private static void SavePipeline(ThumbnailSettings s)
        {
            RenderPipelineType pipeline = RenderPipelineDetector.Current;

            if (pipeline == RenderPipelineType.HDRP || pipeline == RenderPipelineType.URP)
            {
                if (s.VolumeProfileRef != null)
                {
                    EditorPrefs.SetString(k_Prefix + "VolumeProfile", AssetDatabase.GetAssetPath(s.VolumeProfileRef));
                }
            }
            else if (RenderPipelineDetector.HasPostProcessingStackV2)
            {
                if (s.PostProcessProfileRef != null)
                {
                    EditorPrefs.SetString(k_Prefix + "PostProcessProfile", AssetDatabase.GetAssetPath(s.PostProcessProfileRef));
                }
            }
        }

        private static void LoadCamera(ThumbnailSettings s)
        {
            s.OrbitDistance = EditorPrefs.GetFloat(k_Prefix + "OrbitDistance", s.OrbitDistance);

            float pitch = EditorPrefs.GetFloat(k_Prefix + "OrbitPitch", s.OrbitAngles.x);
            float yaw = EditorPrefs.GetFloat(k_Prefix + "OrbitYaw", s.OrbitAngles.y);
            s.OrbitAngles = new Vector2(pitch, yaw);

            float posX = float.Parse(EditorPrefs.GetString(k_Prefix + "PositionX", s.Position.x.ToString()));
            float posY = float.Parse(EditorPrefs.GetString(k_Prefix + "PositionY", s.Position.y.ToString()));
            float posZ = float.Parse(EditorPrefs.GetString(k_Prefix + "PositionZ", s.Position.z.ToString()));
            s.Position = new Vector3(posX, posY, posZ);

            s.OrthographicView = EditorPrefs.GetInt(k_Prefix + "OrthographicView", 0) == 1;
        }

        private static void LoadLight(ThumbnailSettings s)
        {
            float lightYaw = EditorPrefs.GetFloat(k_Prefix + "LightYaw", s.LightRotation.y);
            float lightPitch = EditorPrefs.GetFloat(k_Prefix + "LightPitch", s.LightRotation.x);
            s.LightRotation = new Vector3(lightPitch, lightYaw, 0f);
            s.LightIntensity = EditorPrefs.GetFloat(k_Prefix + "LightIntensity", s.LightIntensity);
        }

        private static void LoadAmbient(ThumbnailSettings s)
        {
            s.AmbientMode = (AmbientMode)EditorPrefs.GetInt(k_Prefix + "AmbientMode", (int)s.AmbientMode);

            float r = float.Parse(EditorPrefs.GetString(k_Prefix + "AmbientColorR", s.AmbientColor.r.ToString()));
            float g = float.Parse(EditorPrefs.GetString(k_Prefix + "AmbientColorG", s.AmbientColor.g.ToString()));
            float b = float.Parse(EditorPrefs.GetString(k_Prefix + "AmbientColorB", s.AmbientColor.b.ToString()));
            float a = float.Parse(EditorPrefs.GetString(k_Prefix + "AmbientColorA", s.AmbientColor.a.ToString()));
            s.AmbientColor = new Color(r, g, b, a);

            s.AmbientIntensity = EditorPrefs.GetFloat(k_Prefix + "AmbientIntensity", s.AmbientIntensity);
        }

        private static void LoadRender(ThumbnailSettings s)
        {
            float r = float.Parse(EditorPrefs.GetString(k_Prefix + "BgColorR", s.BackgroundColor.r.ToString()));
            float g = float.Parse(EditorPrefs.GetString(k_Prefix + "BgColorG", s.BackgroundColor.g.ToString()));
            float b = float.Parse(EditorPrefs.GetString(k_Prefix + "BgColorB", s.BackgroundColor.b.ToString()));
            float a = float.Parse(EditorPrefs.GetString(k_Prefix + "BgColorA", s.BackgroundColor.a.ToString()));
            s.BackgroundColor = new Color(r, g, b, a);

            s.AntiAliasing = (ThumbnailSettings.AntiAliasingMode)EditorPrefs.GetInt(k_Prefix + "AntiAliasing", (int)s.AntiAliasing);
        }

        private static void LoadExport(out int outputResolution, out string lastExportPath)
        {
            outputResolution = EditorPrefs.GetInt(k_Prefix + "OutputResolution", 2);
            lastExportPath = EditorPrefs.GetString(k_Prefix + "LastExportPath", "");
        }

        private static void LoadPipeline(ThumbnailSettings s)
        {
            RenderPipelineType pipeline = RenderPipelineDetector.Current;

            if (pipeline == RenderPipelineType.HDRP || pipeline == RenderPipelineType.URP)
            {
                string path = EditorPrefs.GetString(k_Prefix + "VolumeProfile", "");
                if (!string.IsNullOrEmpty(path))
                {
                    Type t = ReflectionHelper.FindType("UnityEngine.Rendering.VolumeProfile");
                    if (t != null)
                    {
                        s.VolumeProfileRef = AssetDatabase.LoadAssetAtPath(path, t) as ScriptableObject;
                    }
                }
            }
            else if (RenderPipelineDetector.HasPostProcessingStackV2)
            {
                string path = EditorPrefs.GetString(k_Prefix + "PostProcessProfile", "");
                if (!string.IsNullOrEmpty(path))
                {
                    Type t = ReflectionHelper.FindType("UnityEngine.Rendering.PostProcessing.PostProcessProfile");
                    if (t != null)
                    {
                        s.PostProcessProfileRef = AssetDatabase.LoadAssetAtPath(path, t) as ScriptableObject;
                    }
                }
            }
        }
    }
}