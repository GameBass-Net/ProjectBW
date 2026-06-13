/// <summary>
/// Project : Mind Code Interactive
/// Class : ManagerLocator.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Constants;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Abstracts;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_HIGH_PRIORITY)]
    public static class ManagerLocator
    {
        private static readonly ManagerRegistry s_registry = new ManagerRegistry();
        private static List<ISettings> s_settingsCache = new List<ISettings>();
        private static Transform s_managersRoot;
        private static bool s_isInitialized;

        public static IReadOnlyDictionary<Type, IManager> Managers => s_registry.Managers;

        public static Transform ManagersRoot
        {
            get
            {
                if (s_managersRoot == null)
                {
                    CreateInstanceContainer();
                }

                return s_managersRoot;
            }
        }

        public static bool IsInitialized => s_isInitialized;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Cleanup();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialize()
        {
            if (Application.isPlaying)
            {
                Initialize();
            }
        }

        public static void Initialize()
        {
            if (s_isInitialized || !Application.isPlaying)
            {
                return;
            }

            try
            {
                CreateInstanceContainer();
                LoadAllSettings();
                RegisterExistingManagers();
                CreateManagersFromSettings();
                InitializeRegisteredManagers();
                s_isInitialized = true;

                Application.quitting += Cleanup;
            }
            catch (Exception exception)
            {
                Debug.LogError("Failed to initialize manager system: " + exception.Message);
                throw;
            }
        }

        public static void RegisterManager(IManager manager)
        {
            if (manager == null)
            {
                return;
            }

            s_registry.RegisterManager(manager);

            if (manager is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.transform.SetParent(ManagersRoot);
            }

            if (s_isInitialized && !manager.IsInitialized && ShouldAutoInitialize(manager))
            {
                try
                {
                    manager.Initialize();
                }
                catch (Exception exception)
                {
                    Debug.LogError("Failed to initialize registered manager " + manager.Name + ": " + exception.Message);
                }
            }
        }

        public static void UnregisterManager(IManager manager) => s_registry.UnregisterManager(manager);

        public static T GetManager<T>() where T : class, IManager => s_registry.GetManager<T>();

        public static TSettings GetSettings<TManager, TSettings>()
            where TManager : Manager<TSettings>
            where TSettings : Settings<TSettings>
        {
            TManager manager = GetManager<TManager>();
            return manager?.Settings != null ? manager.Settings : SettingsLoader.GetSettings<TSettings>();
        }

        private static void CreateInstanceContainer()
        {
            if (s_managersRoot != null || !Application.isPlaying)
            {
                return;
            }

            GameObject instancesGameObject = new GameObject("Instances");
            instancesGameObject.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(instancesGameObject);
            s_managersRoot = instancesGameObject.transform;
        }

        private static void LoadAllSettings() => s_settingsCache = SettingsLoader.LoadAllSettings();

        private static void RegisterExistingManagers()
        {
#if UNITY_2023_1_OR_NEWER
#pragma warning disable CS0618
            List<IManager> existingManagers =
                UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .OfType<IManager>()
                    .ToList();
#pragma warning restore CS0618
#elif UNITY_2020_1_OR_NEWER
            List<IManager> existingManagers =
                UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true)
                    .OfType<IManager>()
                    .ToList();
#else
            List<IManager> existingManagers =
                Object.FindObjectsOfType<MonoBehaviour>()
                    .OfType<IManager>()
                    .ToList();
#endif

            foreach (IManager manager in existingManagers)
            {
                if (!s_registry.HasManager(manager.GetType()))
                {
                    s_registry.RegisterManager(manager);
                }

                LoadSettingsForExistingManager(manager);
            }
        }

        private static void LoadSettingsForExistingManager(IManager manager)
        {
            if (!(manager is MonoBehaviour monoBehaviour))
            {
                return;
            }

            Type managerType = manager.GetType();
            FieldInfo settingsField = ReflectionCache.GetSettingsField(managerType);

            if (settingsField == null)
            {
                Debug.LogWarning("No settings field found for: " + managerType.Name);
                return;
            }

            object currentSettings = settingsField.GetValue(monoBehaviour);
            if (currentSettings != null)
            {
                return;
            }

            ISettings settings = SettingsLoader.GetSettingsForManager(managerType);
            if (settings is ScriptableObject scriptableObject)
            {
                settingsField.SetValue(monoBehaviour, scriptableObject);
            }
        }

        private static void CreateManagersFromSettings()
        {
            List<ISettings> orderedSettings = s_settingsCache
                .Where(settings => settings.AutoInitialize && settings.AutoCreate && !s_registry.HasManager(settings.GetManagerType()))
                .OrderBy(settings => settings.Priority)
                .ThenBy(settings => settings.ManagerName, StringComparer.Ordinal)
                .ToList();

            foreach (ISettings settings in orderedSettings)
            {
                try
                {
                    CreateManagerFromSettings(settings);
                }
                catch (Exception exception)
                {
                    Debug.LogError("Failed to create manager for " + settings.ManagerName + ": " + exception.Message);
                }
            }
        }

        private static void CreateManagerFromSettings(ISettings settings)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Type managerType = settings.GetManagerType();
            if (s_registry.HasManager(managerType))
            {
                return;
            }

            GameObject gameObject = new GameObject(settings.ManagerName);
            gameObject.transform.SetParent(ManagersRoot);

            IManager manager = gameObject.AddComponent(managerType) as IManager;
            if (manager == null)
            {
                return;
            }

            if (manager is MonoBehaviour monoBehaviour && settings is ScriptableObject scriptableObject)
            {
                FieldInfo settingsField = ReflectionCache.GetSettingsField(managerType);
                settingsField?.SetValue(monoBehaviour, scriptableObject);
            }
        }

        private static void InitializeRegisteredManagers()
        {
            List<IManager> managersToInit = s_registry.GetAllManagers()
                .Where(manager => !manager.IsInitialized && ShouldAutoInitialize(manager))
                .OrderBy(GetManagerPriority)
                .ToList();

            foreach (IManager manager in managersToInit)
            {
                try
                {
                    manager.Initialize();
                }
                catch (Exception exception)
                {
                    Debug.LogError("Failed to initialize " + manager.Name + ": " + exception.Message);
                }
            }
        }

        private static bool ShouldAutoInitialize(IManager manager)
        {
            ISettings settings = SettingsLoader.GetSettingsForManager(manager.GetType());
            return settings?.AutoInitialize ?? false;
        }

        private static int GetManagerPriority(IManager manager)
        {
            ISettings settings = SettingsLoader.GetSettingsForManager(manager.GetType());
            return settings?.Priority ?? 0;
        }

        private static void Cleanup()
        {
            try
            {
                List<IManager> managersToShutdown = s_registry.GetAllManagers()
                    .OrderByDescending(GetManagerPriority)
                    .ToList();

                foreach (IManager manager in managersToShutdown)
                {
                    try
                    {
                        manager.Shutdown();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError("Error shutting down " + manager.Name + ": " + exception.Message);
                    }
                }

                s_registry.Clear();
                s_settingsCache.Clear();
                SettingsLoader.ClearCache();
                ReflectionCache.ClearCache();
                s_isInitialized = false;

                if (!Application.isPlaying && s_managersRoot != null)
                {
                    GameObject root = s_managersRoot.parent?.gameObject;
                    if (root != null)
                    {
                        UnityEngine.Object.DestroyImmediate(root);
                    }
                }

                s_managersRoot = null;
            }
            catch (Exception exception)
            {
                Debug.LogError("Error during cleanup: " + exception.Message);
            }
        }
    }
}