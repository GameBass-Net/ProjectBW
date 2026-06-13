/// <summary>
/// Project : Mind Code Interactive
/// Class : DebugRendererManager.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Core.Debugging
{
    public static class DebugRendererManager
    {
        private static readonly List<IDebuggable> s_Debuggables = new List<IDebuggable>(64);

#if UNITY_EDITOR
        private static readonly List<IDebuggable> s_EditModeCache = new List<IDebuggable>(64);
        private static bool s_EditModeCacheDirty = true;
        private static int s_LastFrameRendered = -1;
#endif

        private static DebugRendererRunner s_Runner;
        private static bool s_IsSrpHooked;
        private static bool s_GlobalDebugEnabled = true;

        public static bool GlobalDebugEnabled
        {
            get => s_GlobalDebugEnabled;
            set => s_GlobalDebugEnabled = value;
        }

        public static void Register(IDebuggable debuggable)
        {
            if (debuggable == null || s_Debuggables.Contains(debuggable))
            {
                return;
            }

            s_Debuggables.Add(debuggable);
        }

        public static void Unregister(IDebuggable debuggable)
        {
            if (debuggable == null)
            {
                return;
            }

            s_Debuggables.Remove(debuggable);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitRuntime()
        {
            if (!Application.isEditor)
            {
                return;
            }

            DebugRenderer.InitMaterial();

            EnsureRunner();
            HookSrp();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitEditor()
        {
            DebugRenderer.InitMaterial();

            EnsureRunner();
            HookSrp();

            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;

            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

            UnityEditor.AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
            UnityEditor.AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnEditorSceneOpened;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnEditorSceneOpened;

            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed -= OnEditorSceneClosed;
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += OnEditorSceneClosed;

            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= OnEditorActiveSceneChanged;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += OnEditorActiveSceneChanged;

            UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;

            s_EditModeCacheDirty = true;

            UnityEditor.SceneView.RepaintAll();
        }

        private static void OnHierarchyChanged()
        {
            s_EditModeCacheDirty = true;
        }

        private static void OnEditorSceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            DebugRenderer.ClearAll();
            s_Debuggables.Clear();
            s_EditModeCacheDirty = true;
            UnityEditor.SceneView.RepaintAll();
        }

        private static void OnEditorSceneClosed(UnityEngine.SceneManagement.Scene scene)
        {
            DebugRenderer.ClearAll();
            s_Debuggables.Clear();
            s_EditModeCacheDirty = true;
        }

        private static void OnEditorActiveSceneChanged(UnityEngine.SceneManagement.Scene previous, UnityEngine.SceneManagement.Scene current)
        {
            DebugRenderer.ClearAll();
            s_Debuggables.Clear();
            s_EditModeCacheDirty = true;
            UnityEditor.SceneView.RepaintAll();
        }

        private static void OnBeforeAssemblyReload()
        {
            UnhookSrp();
            DebugRenderer.ClearAll();
            DebugRenderer.ForceRebuildMaterials();
            s_Debuggables.Clear();
        }

        private static void OnAfterAssemblyReload()
        {
            DebugRenderer.ClearAll();
            DebugRenderer.ForceRebuildMaterials();
            UnityEditor.SceneView.RepaintAll();
        }

        private static void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            DebugRenderer.ForceRebuildMaterials();
            DebugRenderer.ClearAll();

            if (state != UnityEditor.PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            s_Runner = null;
            s_IsSrpHooked = false;
            s_GlobalDebugEnabled = true;
            s_Debuggables.Clear();

            EnsureRunner();
            HookSrp();
        }
#endif

        private static void EnsureRunner()
        {
            if (s_Runner != null)
            {
                return;
            }

#if UNITY_2023_1_OR_NEWER
#pragma warning disable CS0618
            DebugRendererRunner[] runners = Object.FindObjectsByType<DebugRendererRunner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#pragma warning restore CS0618
#else
            DebugRendererRunner[] runners = Object.FindObjectsOfType<DebugRendererRunner>(true);
#endif

            DebugRendererRunner validRunner = null;

            for (int i = 0; i < runners.Length; i++)
            {
                DebugRendererRunner runner = runners[i];

                if (runner == null)
                {
                    continue;
                }

                if (validRunner == null)
                {
                    validRunner = runner;
                    continue;
                }

                DestroyRunner(runner.gameObject);
            }

            if (validRunner != null)
            {
                s_Runner = validRunner;

                if (!Application.isPlaying)
                {
                    s_Runner.gameObject.hideFlags = HideFlags.HideAndDontSave;
                }

                return;
            }

            GameObject runnerObject = new GameObject("DebugRenderer_Runner");

            if (!Application.isPlaying)
            {
                runnerObject.hideFlags = HideFlags.HideAndDontSave;
            }

            s_Runner = runnerObject.AddComponent<DebugRendererRunner>();

            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(runnerObject);
            }
        }

        private static void DestroyRunner(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }

        private static void HookSrp()
        {
            if (s_IsSrpHooked)
            {
                return;
            }

            RenderPipelineManager.endCameraRendering -= OnSrpEndCamera;
            RenderPipelineManager.endCameraRendering += OnSrpEndCamera;

            s_IsSrpHooked = true;

#if UNITY_EDITOR
            int handlerCount = CountSrpHandlers();
            if (handlerCount > 1)
            {
                Debug.LogWarning("[DebugRendererManager] " + handlerCount
                    + " SRP endCameraRendering handlers attached (expected 1). Possible handler leak.");
            }
#endif
        }

#if UNITY_EDITOR
        private static int CountSrpHandlers()
        {
            System.Reflection.FieldInfo field = typeof(RenderPipelineManager).GetField(
                "endCameraRendering",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            if (field == null)
            {
                return -1;
            }

            System.Delegate del = field.GetValue(null) as System.Delegate;
            if (del == null)
            {
                return 0;
            }

            System.Delegate[] invocationList = del.GetInvocationList();
            int count = 0;
            for (int i = 0; i < invocationList.Length; i++)
            {
                if (invocationList[i].Method.Name == nameof(OnSrpEndCamera))
                {
                    count++;
                }
            }

            return count;
        }
#endif

        private static void UnhookSrp()
        {
            RenderPipelineManager.endCameraRendering -= OnSrpEndCamera;
            s_IsSrpHooked = false;
        }

        private static void OnSrpEndCamera(ScriptableRenderContext context, Camera camera)
        {
            if (!s_GlobalDebugEnabled || !IsSupportedCamera(camera))
            {
                return;
            }

            if (Application.isPlaying)
            {
                ProcessRuntime(camera);
                return;
            }

            ProcessEditMode(camera);
        }

        private static void ProcessRuntime(Camera camera)
        {
            if (camera == null || !s_GlobalDebugEnabled)
            {
                return;
            }

            DebugRenderer.ViewFlags currentView = GetCurrentView(camera);

            if (currentView == DebugRenderer.ViewFlags.None)
            {
                return;
            }

            DebugRenderer.ClearAll();

            for (int i = s_Debuggables.Count - 1; i >= 0; i--)
            {
                IDebuggable debuggable = s_Debuggables[i];

                if (debuggable is Object unityObject && unityObject == null)
                {
                    s_Debuggables.RemoveAt(i);
                    continue;
                }

                if (!debuggable.DebugEnabled || !PassesFilter(debuggable.DebugFlags, currentView))
                {
                    continue;
                }

#if UNITY_EDITOR
                if (debuggable.RequireSelection && debuggable is MonoBehaviour mb && !IsSelected(mb.gameObject))
                {
                    continue;
                }
#endif

                debuggable.OnDebugRender();
            }

            DebugRenderer.RenderForCamera(camera);
        }

        private static void ProcessEditMode(Camera camera)
        {
            if (camera == null || !s_GlobalDebugEnabled)
            {
                return;
            }

            DebugRenderer.ViewFlags currentView = GetCurrentView(camera);

            if (currentView == DebugRenderer.ViewFlags.None)
            {
                return;
            }

#if UNITY_EDITOR
            int currentFrame = Time.frameCount;
            if (s_LastFrameRendered == currentFrame && camera.cameraType == CameraType.SceneView)
            {
                DebugRenderer.RenderForCamera(camera);
                return;
            }
            s_LastFrameRendered = currentFrame;

            if (s_EditModeCacheDirty)
            {
                RebuildEditModeCache();
            }

            DebugRenderer.ClearAll();

            for (int i = s_EditModeCache.Count - 1; i >= 0; i--)
            {
                IDebuggable debuggable = s_EditModeCache[i];

                if (debuggable is Object unityObject && unityObject == null)
                {
                    s_EditModeCache.RemoveAt(i);
                    continue;
                }

                MonoBehaviour behaviour = debuggable as MonoBehaviour;
                if (behaviour == null || !behaviour.isActiveAndEnabled)
                {
                    continue;
                }

                if (debuggable.RequireSelection && !IsSelected(behaviour.gameObject))
                {
                    continue;
                }

                if (!debuggable.DebugEnabled || !PassesFilter(debuggable.DebugFlags, currentView))
                {
                    continue;
                }

                debuggable.OnDebugRender();
            }

            DebugRenderer.RenderForCamera(camera);
#endif
        }

#if UNITY_EDITOR
        private static void RebuildEditModeCache()
        {
            s_EditModeCache.Clear();

#if UNITY_2023_1_OR_NEWER
#pragma warning disable CS0618
            MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#pragma warning restore CS0618
#else
            MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(false);
#endif

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IDebuggable debuggable)
                {
                    s_EditModeCache.Add(debuggable);
                }
            }

            s_EditModeCacheDirty = false;
        }
#endif

#if UNITY_EDITOR
        private static bool IsSelected(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            foreach (Object selected in UnityEditor.Selection.objects)
            {
                if (selected is GameObject selectedGO)
                {
                    if (selectedGO == go || go.transform.IsChildOf(selectedGO.transform))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
#endif

        private static bool IsSupportedCamera(Camera camera)
        {
            if (camera == null)
            {
                return false;
            }

            if (camera.cameraType != CameraType.SceneView && camera.cameraType != CameraType.Game)
            {
                return false;
            }

            string cameraName = camera.name;

            if (!string.IsNullOrEmpty(cameraName) && cameraName.Contains("Preview"))
            {
                return false;
            }

            return true;
        }

        private static DebugRenderer.ViewFlags GetCurrentView(Camera camera)
        {
            if (camera == null)
            {
                return DebugRenderer.ViewFlags.None;
            }

            if (camera.cameraType == CameraType.SceneView)
            {
                return DebugRenderer.ViewFlags.SceneView;
            }

            if (camera.cameraType == CameraType.Game)
            {
                return DebugRenderer.ViewFlags.GameView;
            }

            return DebugRenderer.ViewFlags.None;
        }

        private static bool PassesFilter(DebugRenderer.ViewFlags requiredFlags, DebugRenderer.ViewFlags currentFlags)
        {
            if (requiredFlags == DebugRenderer.ViewFlags.None)
            {
                return false;
            }

            return (requiredFlags & currentFlags) != 0;
        }

        [ExecuteAlways]
        private sealed class DebugRendererRunner : MonoBehaviour
        {
            private void OnEnable()
            {
#if UNITY_EDITOR
                UnityEditor.SceneView.RepaintAll();
#endif
            }

            private void OnRenderObject()
            {
                if (GraphicsSettings.currentRenderPipeline != null || !s_GlobalDebugEnabled)
                {
                    return;
                }

                Camera currentCamera = Camera.current;

                if (!IsSupportedCamera(currentCamera))
                {
                    return;
                }

                if (Application.isPlaying)
                {
                    ProcessRuntime(currentCamera);
                    return;
                }

                ProcessEditMode(currentCamera);
            }
        }
    }
}