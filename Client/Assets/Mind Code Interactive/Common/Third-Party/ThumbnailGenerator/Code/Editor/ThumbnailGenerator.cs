/// <summary>
/// Project : Thumbnail Generator
/// Class : ThumbnailGenerator.cs
/// Namespace : MindCodeInteractive.ThumbnailGenerator.Code.Editor
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;

namespace MindCodeInteractive.ThumbnailGenerator.Code.Editor
{
    public enum RenderPipelineType { BuiltIn, URP, HDRP }

    public class ThumbnailSettings : ScriptableObject
    {
        public enum AntiAliasingMode { None = 1, MSAA_2 = 2, MSAA_4 = 4, MSAA_8 = 8 }

        public Vector2 OrbitAngles = new Vector2(20f, 135f);
        public float OrbitDistance = 1.5f;
        public Vector3 Position = Vector3.zero;
        public Color BackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0f);
        public bool OrthographicView = false;
        public AntiAliasingMode AntiAliasing = AntiAliasingMode.MSAA_2;
        public bool NonReadableTexture = true;
        public Vector3 LightRotation = new Vector3(120f, 90f, 0f);
        public float LightIntensity = 1f;
        public AmbientMode AmbientMode = AmbientMode.Flat;
        public Color AmbientColor = Color.white;
        public float AmbientIntensity = 1f;

        public ScriptableObject VolumeProfileRef;
        public ScriptableObject PostProcessProfileRef;
    }

    public static class RenderPipelineDetector
    {
        private static RenderPipelineType? s_cached;
        private static bool? s_hasPPv2;

        public static RenderPipelineType Current
        {
            get
            {
                if (s_cached.HasValue)
                {
                    return s_cached.Value;
                }

                RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
                if (pipeline == null)
                {
                    s_cached = RenderPipelineType.BuiltIn;
                }
                else
                {
                    string typeName = pipeline.GetType().Name;
                    if (typeName.Contains("HDRenderPipelineAsset"))
                    {
                        s_cached = RenderPipelineType.HDRP;
                    }
                    else if (typeName.Contains("UniversalRenderPipelineAsset"))
                    {
                        s_cached = RenderPipelineType.URP;
                    }
                    else
                    {
                        s_cached = RenderPipelineType.BuiltIn;
                    }
                }
                return s_cached.Value;
            }
        }

        public static bool HasPostProcessingStackV2
        {
            get
            {
                if (s_hasPPv2.HasValue)
                {
                    return s_hasPPv2.Value;
                }

                s_hasPPv2 = false;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if (asm.GetType("UnityEngine.Rendering.PostProcessing.PostProcessVolume", false) != null)
                        {
                            s_hasPPv2 = true;
                            break;
                        }
                    }
                    catch { }
                }
                return s_hasPPv2.Value;
            }
        }

        public static void InvalidateCache()
        {
            s_cached = null;
            s_hasPPv2 = null;
        }
    }

    public static class ReflectionHelper
    {
        private static readonly Dictionary<string, Type> s_typeCache = new Dictionary<string, Type>();

        public static Type FindType(string fullName)
        {
            if (s_typeCache.TryGetValue(fullName, out Type cached))
            {
                return cached;
            }

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type t = asm.GetType(fullName, false);
                    if (t != null)
                    {
                        s_typeCache[fullName] = t;
                        return t;
                    }
                }
                catch { }
            }
            s_typeCache[fullName] = null;
            return null;
        }

        public static Component GetOrAddComponent(GameObject go, string typeName)
        {
            Type t = FindType(typeName);
            if (t == null)
            {
                return null;
            }

            Component c = go.GetComponent(t);
            if (c == null)
            {
                c = go.AddComponent(t);
            }

            return c;
        }

        public static void SetProperty(object target, string propName, object value)
        {
            if (target == null)
            {
                return;
            }

            PropertyInfo prop = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
                return;
            }
            FieldInfo field = target.GetType().GetField(propName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        public static object GetProperty(object target, string propName)
        {
            if (target == null)
            {
                return null;
            }

            PropertyInfo prop = target.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null)
            {
                return prop.GetValue(target);
            }

            FieldInfo field = target.GetType().GetField(propName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                return field.GetValue(target);
            }

            return null;
        }

        public static object InvokeMethod(object target, string methodName, params object[] args)
        {
            if (target == null)
            {
                return null;
            }

            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                return method.Invoke(target, args);
            }

            return null;
        }

        public static object InvokeStaticMethod(string typeName, string methodName, params object[] args)
        {
            Type t = FindType(typeName);
            if (t == null)
            {
                return null;
            }

            MethodInfo method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method != null)
            {
                return method.Invoke(null, args);
            }

            return null;
        }

        public static object InvokeExtensionMethod(string typeName, string methodName, object target, params object[] args)
        {
            Type t = FindType(typeName);
            if (t == null)
            {
                return null;
            }

            MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (MethodInfo m in methods)
            {
                if (m.Name == methodName && m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
                {
                    ParameterInfo[] parameters = m.GetParameters();
                    if (parameters.Length >= 1 && parameters[0].ParameterType.IsInstanceOfType(target))
                    {
                        object[] fullArgs = new object[args.Length + 1];
                        fullArgs[0] = target;
                        Array.Copy(args, 0, fullArgs, 1, args.Length);
                        return m.Invoke(null, fullArgs);
                    }
                }
            }
            return null;
        }
    }

    public static class ThumbnailGenerator
    {
        private const string k_PreviewLayerName = "ThumbnailPreview";
        private const string k_PreviewObjectName = "PreviewObject";
        private const string k_PreviewVolumeName = "PreviewVolume";
        private const string k_PreviewPostProcessVolumeName = "PreviewPostProcessVolume";

        private static readonly Vector3 s_defaultHiddenPosition = new Vector3(-250f, -250f, -250f);

        private static List<Renderer> s_rendererBuffer;
        private static Component s_postProcessLayer;
        private static GameObject s_postProcessVolumeGO;
        private static Component s_cachedVolumeComponent;

        public static ThumbnailSettings Settings { get; set; }
        public static Camera CachedCamera { get; set; }
        public static Light CachedLight { get; set; }

        public static void Initialize()
        {
            if (Settings != null)
            {
                Object.DestroyImmediate(Settings);
            }

            try
            {
                RenderPipelineDetector.InvalidateCache();
                Settings = ScriptableObject.CreateInstance<ThumbnailSettings>();
                Settings.hideFlags = HideFlags.HideAndDontSave;

#if UNITY_EDITOR
                AddLayerIfNotExists(k_PreviewLayerName);
#endif

                SetupCamera();
                SetupLight();
                SetupVolume();
            }
            catch
            {
                Cleanup();
            }
        }

        public static void Cleanup()
        {
            ClearPostProcessing();

            if (CachedCamera != null)
            {
                Object.DestroyImmediate(CachedCamera.gameObject);
            }

            if (CachedLight != null)
            {
                Object.DestroyImmediate(CachedLight.gameObject);
            }

            if (s_cachedVolumeComponent != null)
            {
                Object.DestroyImmediate(s_cachedVolumeComponent.gameObject);
                s_cachedVolumeComponent = null;
            }

            if (Settings != null)
            {
                Object.DestroyImmediate(Settings);
            }

            CachedCamera = null;
            CachedLight = null;

#if UNITY_EDITOR
            RemoveLayerIfExists(k_PreviewLayerName);
#endif
        }

        private static void SetupCamera()
        {
            GameObject cameraPrefab = LoadPreviewPrefab("Camera");
            GameObject cameraGO;

            if (cameraPrefab != null)
            {
                cameraGO = Object.Instantiate(cameraPrefab);
                CachedCamera = cameraGO.GetComponent<Camera>();
            }
            else
            {
                cameraGO = new GameObject("ThumbnailPreviewCamera");
                CachedCamera = cameraGO.AddComponent<Camera>();
                CachedCamera.fieldOfView = 60f;

                // In HDRP, HDAdditionalCameraData is added automatically by the pipeline
                // when a Camera component is added — but only after a frame tick.
                // Force it now by calling the HDRP extension method if available.
                ReflectionHelper.InvokeStaticMethod(
                    "UnityEngine.Rendering.HighDefinition.HDCameraExtensions",
                    "GetOrCreateHDAdditionalCameraData",
                    CachedCamera);
            }

            cameraGO.name = "ThumbnailPreviewCamera";
            cameraGO.layer = GetPreviewLayer();
            cameraGO.hideFlags = HideFlags.HideAndDontSave;
            CachedCamera.enabled = false;
            CachedCamera.cullingMask = 1 << GetPreviewLayer();

            ApplyCameraConfig();
        }

        private static void SetupLight()
        {
            GameObject lightPrefab = LoadPreviewPrefab("Light");
            GameObject lightGO;

            if (lightPrefab != null)
            {
                lightGO = Object.Instantiate(lightPrefab);
                CachedLight = lightGO.GetComponent<Light>();
            }
            else
            {
                lightGO = new GameObject("ThumbnailPreviewLight");
                CachedLight = lightGO.AddComponent<Light>();

                // Force HDAdditionalLightData creation in HDRP.
                ReflectionHelper.InvokeStaticMethod(
                    "UnityEngine.Rendering.HighDefinition.HDLightExtensions",
                    "GetOrCreateHDAdditionalLightData",
                    CachedLight);
            }

            lightGO.name = "ThumbnailPreviewLight";
            lightGO.layer = GetPreviewLayer();
            lightGO.hideFlags = HideFlags.HideAndDontSave;
            CachedLight.type = LightType.Directional;
            CachedLight.shadows = LightShadows.Soft;
            CachedLight.intensity = Settings.LightIntensity;
            CachedLight.renderMode = LightRenderMode.ForceVertex;
            CachedLight.cullingMask = 1 << GetPreviewLayer();
            CachedLight.enabled = false;

            ApplyLightConfig();
        }

        private static void SetupVolume()
        {
            RenderPipelineType pipelineType = RenderPipelineDetector.Current;
            if (pipelineType == RenderPipelineType.BuiltIn)
            {
                return;
            }

            Type volumeType = ReflectionHelper.FindType("UnityEngine.Rendering.Volume");
            if (volumeType == null)
            {
                return;
            }

            GameObject volumeObject = new GameObject(k_PreviewVolumeName);
            volumeObject.hideFlags = HideFlags.HideAndDontSave;
            volumeObject.layer = GetPreviewLayer();

            s_cachedVolumeComponent = volumeObject.AddComponent(volumeType);
            ReflectionHelper.SetProperty(s_cachedVolumeComponent, "isGlobal", true);
            ReflectionHelper.SetProperty(s_cachedVolumeComponent, "sharedProfile", Settings.VolumeProfileRef);
            (s_cachedVolumeComponent as Behaviour).enabled = false;
        }

        private static void ApplyCameraConfig()
        {
            switch (RenderPipelineDetector.Current)
            {
                case RenderPipelineType.HDRP:
                    ApplyCameraConfigHDRP();
                    break;
                case RenderPipelineType.URP:
                    ApplyCameraConfigURP();
                    break;
                default:
                    ApplyCameraConfigBuiltIn();
                    break;
            }
        }

        private static void ApplyLightConfig()
        {
            if (RenderPipelineDetector.Current == RenderPipelineType.HDRP)
            {
                ApplyLightConfigHDRP();
            }
        }

        private static void ApplyCameraConfigHDRP()
        {
            // HDAdditionalCameraData must NOT be added via AddComponent in HDRP —
            // HDRP adds it automatically when a Camera is created. We only GetComponent.
            // If it's missing, HDRP hasn't initialised yet; skip gracefully.
            Component hdCamData = CachedCamera.gameObject.GetComponent(
                ReflectionHelper.FindType("UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData")
                ?? typeof(Component));

            if (hdCamData == null)
            {
                // Fallback: set standard camera flags so at least something renders.
                CachedCamera.clearFlags = CameraClearFlags.SolidColor;
                CachedCamera.backgroundColor = Settings.BackgroundColor;
                return;
            }

            Type antiAliasingType = ReflectionHelper.FindType(
                "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData+AntialiasingMode");
            if (antiAliasingType != null)
            {
                try
                {
                    object msaaValue = Enum.Parse(antiAliasingType, "MSAA");
                    ReflectionHelper.SetProperty(hdCamData, "antialiasing", msaaValue);
                }
                catch { }
            }

            try { ReflectionHelper.SetProperty(hdCamData, "msaaSampleCount", (int)Settings.AntiAliasing); } catch { }

            Type clearColorType = ReflectionHelper.FindType(
                "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData+ClearColorMode");
            if (clearColorType != null)
            {
                try
                {
                    object colorValue = Enum.Parse(clearColorType, "Color");
                    ReflectionHelper.SetProperty(hdCamData, "clearColorMode", colorValue);
                }
                catch { }
            }

            try { ReflectionHelper.SetProperty(hdCamData, "backgroundColorHDR", Settings.BackgroundColor); } catch { }
        }

        private static void ApplyLightConfigHDRP()
        {
            // Same as camera: HDAdditionalLightData is added automatically by HDRP.
            Type hdLightType = ReflectionHelper.FindType(
                "UnityEngine.Rendering.HighDefinition.HDAdditionalLightData");
            if (hdLightType == null)
            {
                return;
            }

            Component hdLightData = CachedLight.gameObject.GetComponent(hdLightType);
            if (hdLightData == null)
            {
                return;
            }

            Type layerType = ReflectionHelper.FindType("UnityEngine.Rendering.HighDefinition.LightLayerEnum");
            if (layerType != null)
            {
                try
                {
                    object defaultLayer = Enum.Parse(layerType, "LightLayerDefault");
                    ReflectionHelper.SetProperty(hdLightData, "lightlayersMask", defaultLayer);
                }
                catch { }
            }
        }

        private static void ApplyCameraConfigURP()
        {
            CachedCamera.clearFlags = CameraClearFlags.Color;
            CachedCamera.backgroundColor = Settings.BackgroundColor;

            Component urpCamData = ReflectionHelper.InvokeExtensionMethod(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineExtensions",
                "GetUniversalAdditionalCameraData",
                CachedCamera) as Component;

            if (urpCamData == null)
            {
                urpCamData = ReflectionHelper.GetOrAddComponent(
                    CachedCamera.gameObject,
                    "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData");
            }

            if (urpCamData != null)
            {
                ReflectionHelper.SetProperty(urpCamData, "renderPostProcessing", true);
            }
        }

        private static void ApplyCameraConfigBuiltIn()
        {
            CachedCamera.allowMSAA = ((int)Settings.AntiAliasing) > 1;
            CachedCamera.allowHDR = true;
            CachedCamera.clearFlags = CameraClearFlags.SolidColor;
            CachedCamera.backgroundColor = Settings.BackgroundColor;
            CachedCamera.depthTextureMode = DepthTextureMode.Depth;
            CachedCamera.renderingPath = RenderingPath.Forward;
        }

        public static Texture2D CreateModelPreview(GameObject model, int width, int height)
        {
            if (!IsPreviewValid(model, width, height))
            {
                return null;
            }

            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);

            AmbientMode savedAmbientMode = RenderSettings.ambientMode;
            Color savedAmbientColor = RenderSettings.ambientLight;
            float savedAmbientIntensity = RenderSettings.ambientIntensity;

            GameObject previewObject = null;

            try
            {
                previewObject = InstantiateModel(model);
                if (previewObject == null)
                {
                    return null;
                }

                previewObject.transform.position = s_defaultHiddenPosition;

                previewObject.SetActive(false);
                ApplyPreviewLayer(previewObject);

                if (HasStaticComponent(previewObject))
                {
                    previewObject.isStatic = false;
                    Animator animator = previewObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.enabled = false;
                    }
                }

                ConfigureRenderSettings();
                InitializePostProcessing();

                int previewLayerMask = 1 << GetPreviewLayer();
                CachedLight.enabled = true;
                CachedLight.cullingMask = previewLayerMask;
                CachedCamera.enabled = true;
                CachedCamera.cullingMask = previewLayerMask;

                if (s_cachedVolumeComponent != null)
                {
                    (s_cachedVolumeComponent as Behaviour).enabled = true;
                }

                previewObject.transform.position = Vector3.zero;
                previewObject.transform.rotation = Quaternion.identity;

                RemovePreviewLayerFromAllLights();

                previewObject.SetActive(true);

                if (!TryGetRenderableBounds(previewObject, out Bounds bounds))
                {
                    return null;
                }

                RenderToTexture(result, bounds, width, height);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ThumbnailGenerator error: {ex.Message}");
            }
            finally
            {
                if (previewObject != null)
                {
                    Object.DestroyImmediate(previewObject);
                }
            }

            ShutdownPreviewEnvironment();
            RestoreRenderSettings(savedAmbientMode, savedAmbientColor, savedAmbientIntensity);

            return result;
        }

        private static bool IsPreviewValid(GameObject model, int width, int height)
        {
            return model != null && Settings != null && CachedCamera != null &&
                   CachedLight != null && width > 0 && height > 0;
        }

        private static GameObject InstantiateModel(GameObject model)
        {
            GameObject previewObject;

#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(model);

            if (!string.IsNullOrEmpty(assetPath))
            {
                previewObject = (GameObject)PrefabUtility.InstantiatePrefab(model);
            }
            else
            {
                previewObject = Object.Instantiate(model);
            }
#else
            previewObject = Object.Instantiate(model);
#endif

            if (previewObject == null)
            {
                return null;
            }

            previewObject.name = k_PreviewObjectName;
            previewObject.hideFlags = HideFlags.HideAndDontSave;

            return previewObject;
        }

        private static void ConfigureRenderSettings()
        {
            RenderSettings.ambientMode = Settings.AmbientMode;
            RenderSettings.ambientLight = Settings.AmbientColor;
            RenderSettings.ambientIntensity = Settings.AmbientIntensity;
        }

        private static void RenderToTexture(Texture2D result, Bounds bounds, int width, int height)
        {
            Vector3 cameraTarget = bounds.center + CachedCamera.transform.TransformVector(Settings.Position);
            Quaternion cameraRotation = Quaternion.Euler(Settings.OrbitAngles.x, Settings.OrbitAngles.y, 0f);

            CachedLight.transform.rotation = Quaternion.Euler(Settings.LightRotation);
            CachedCamera.orthographic = Settings.OrthographicView;
            CachedCamera.aspect = (float)width / height;
            CachedCamera.depthTextureMode = DepthTextureMode.Depth;

            FitCamera(CachedCamera, cameraRotation, cameraTarget, bounds);

            RenderTexture renderTexture = RenderTexture.GetTemporary(
                width, height, 24, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Default, (int)Settings.AntiAliasing);

            CachedCamera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;

            GL.Clear(true, true, Settings.BackgroundColor);
            CachedCamera.renderingPath = RenderingPath.Forward;
            CachedCamera.Render();

            result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);
            result.Apply(false, Settings.NonReadableTexture);

            CachedCamera.targetTexture = null;
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        private static void FitCamera(Camera camera, Quaternion rotation, Vector3 target, Bounds bounds)
        {
            float boundsSphereRadius = bounds.extents.magnitude;
            float boundsDiagonal = bounds.size.magnitude;
            float zoomFactor = Mathf.Clamp(Settings.OrbitDistance, 0.05f, 10f);

            Vector3 forward = rotation * Vector3.forward;

            if (camera.orthographic)
            {
                float aspect = Mathf.Max(0.0001f, camera.aspect);
                float baseSize = Mathf.Max(bounds.extents.y, bounds.extents.x / aspect);

                camera.orthographicSize = baseSize * zoomFactor;

                float camDistance = boundsDiagonal + boundsSphereRadius + 1f;
                camera.transform.position = target - forward * camDistance;
                camera.transform.LookAt(target, Vector3.up);

                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = camDistance * 2f + boundsDiagonal;
            }
            else
            {
                float fov = camera.fieldOfView * Mathf.Deg2Rad;
                float aspect = Mathf.Max(0.0001f, camera.aspect);

                float distanceForHeight = boundsSphereRadius / Mathf.Tan(fov * 0.5f);
                float distanceForWidth = boundsSphereRadius / (Mathf.Tan(fov * 0.5f) * aspect);
                float finalDistance = Mathf.Max(distanceForHeight, distanceForWidth) * zoomFactor;

                camera.transform.position = target - forward * finalDistance;
                camera.transform.LookAt(target, Vector3.up);

                camera.nearClipPlane = Mathf.Max(0.01f, finalDistance - boundsSphereRadius * 2f);
                camera.farClipPlane = finalDistance + boundsDiagonal * 2f;
            }
        }

        private static void ShutdownPreviewEnvironment()
        {
            CachedCamera.enabled = false;
            CachedLight.enabled = false;

            if (s_cachedVolumeComponent != null)
            {
                (s_cachedVolumeComponent as Behaviour).enabled = false;
            }

            ClearPostProcessing();
        }

        private static void RestoreRenderSettings(AmbientMode mode, Color color, float intensity)
        {
            RenderSettings.ambientMode = mode;
            RenderSettings.ambientLight = color;
            RenderSettings.ambientIntensity = intensity;
        }

        private static bool TryGetRenderableBounds(GameObject root, out Bounds bounds)
        {
            if (s_rendererBuffer == null)
            {
                s_rendererBuffer = new List<Renderer>(64);
            }

            s_rendererBuffer.Clear();
            root.GetComponentsInChildren(true, s_rendererBuffer);

            bounds = new Bounds();
            bool hasBounds = false;

            foreach (Renderer renderer in s_rendererBuffer)
            {
                if (!renderer || !renderer.enabled || renderer is ParticleSystemRenderer)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static bool HasStaticComponent(GameObject obj)
        {
            if (obj.isStatic)
            {
                return true;
            }

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                if (HasStaticComponent(obj.transform.GetChild(i).gameObject))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyPreviewLayer(GameObject obj)
        {
            int layer = GetPreviewLayer();
            obj.layer = layer;

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                ApplyPreviewLayer(obj.transform.GetChild(i).gameObject);
            }
        }

        private static void RemovePreviewLayerFromAllLights()
        {
#if UNITY_2023_1_OR_NEWER
#pragma warning disable CS0618
            Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#pragma warning restore CS0618
#else
            Light[] allLights = Object.FindObjectsOfType<Light>();
#endif

            int previewLayer = GetPreviewLayer();
            if (previewLayer == -1)
            {
                return;
            }

            int layerBit = 1 << previewLayer;

            for (int i = 0; i < allLights.Length; i++)
            {
                Light light = allLights[i];

                if (!light || light == CachedLight)
                {
                    continue;
                }

                if ((light.cullingMask & layerBit) != 0)
                {
                    light.cullingMask &= ~layerBit;
                }
            }
        }

        private static int GetPreviewLayer()
        {
            return LayerMask.NameToLayer(k_PreviewLayerName);
        }

        private static GameObject LoadPreviewPrefab(string type)
        {
            switch (RenderPipelineDetector.Current)
            {
                case RenderPipelineType.HDRP:
                    return Resources.Load<GameObject>($"HDRP Preview{type}");
                case RenderPipelineType.URP:
                    return Resources.Load<GameObject>($"URP Preview{type}");
                default:
                    return Resources.Load<GameObject>($"Preview{type}");
            }
        }

        public static void ClearPostProcessing()
        {
            if (s_postProcessLayer != null)
            {
                Object.DestroyImmediate(s_postProcessLayer);
                s_postProcessLayer = null;
            }

            if (s_postProcessVolumeGO != null)
            {
                Object.DestroyImmediate(s_postProcessVolumeGO);
                s_postProcessVolumeGO = null;
            }
        }

        public static void InitializePostProcessing()
        {
            if (RenderPipelineDetector.Current != RenderPipelineType.BuiltIn)
            {
                return;
            }

            if (!RenderPipelineDetector.HasPostProcessingStackV2)
            {
                return;
            }

            if (CachedCamera == null)
            {
                return;
            }

            if (Settings.PostProcessProfileRef == null)
            {
                return;
            }

            Type ppLayerType = ReflectionHelper.FindType("UnityEngine.Rendering.PostProcessing.PostProcessLayer");
            Type ppVolumeType = ReflectionHelper.FindType("UnityEngine.Rendering.PostProcessing.PostProcessVolume");

            if (ppLayerType == null || ppVolumeType == null)
            {
                return;
            }

            Component ppLayer = CachedCamera.GetComponent(ppLayerType);
            if (ppLayer == null)
            {
                ppLayer = CachedCamera.gameObject.AddComponent(ppLayerType);
                LayerMask volumeLayerMask = 1 << LayerMask.NameToLayer(k_PreviewLayerName);
                ReflectionHelper.SetProperty(ppLayer, "volumeLayer", volumeLayerMask);
                ReflectionHelper.SetProperty(ppLayer, "volumeTrigger", CachedCamera.transform);
            }
            s_postProcessLayer = ppLayer;

            GameObject ppVolumeGo = new GameObject(k_PreviewPostProcessVolumeName);
            ppVolumeGo.layer = GetPreviewLayer();
            ppVolumeGo.hideFlags = HideFlags.HideAndDontSave;

            Component ppVolume = ppVolumeGo.AddComponent(ppVolumeType);
            ReflectionHelper.SetProperty(ppVolume, "profile", Settings.PostProcessProfileRef);
            ReflectionHelper.SetProperty(ppVolume, "isGlobal", true);
            s_postProcessVolumeGO = ppVolumeGo;
        }

        public static void ApplyPostProcessProfile(ScriptableObject profile)
        {
            Settings.PostProcessProfileRef = profile;
        }

        public static void ApplyVolumeProfile(ScriptableObject profile)
        {
            Settings.VolumeProfileRef = profile;
            if (s_cachedVolumeComponent != null)
            {
                ReflectionHelper.SetProperty(s_cachedVolumeComponent, "sharedProfile", profile);
            }
        }

#if UNITY_EDITOR
        private static void AddLayerIfNotExists(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 8; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return;
                }
            }

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layer.stringValue))
                {
                    layer.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
        }

        private static void RemoveLayerIfExists(string layerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                if (layer.stringValue == layerName)
                {
                    layer.stringValue = string.Empty;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
        }
#else
        private static void AddLayerIfNotExists(string layerName) { }
        private static void RemoveLayerIfExists(string layerName) { }
#endif
    }
}