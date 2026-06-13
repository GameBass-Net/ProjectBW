/// <summary>
/// Project : Mind Code Interactive
/// Class : SettingsLoader.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils
{
    public static class SettingsLoader
    {
        private static readonly Dictionary<Type, ISettings> s_settingsCache = new Dictionary<Type, ISettings>();
        private static readonly object s_cacheLock = new object();

        public static List<ISettings> LoadAllSettings()
        {
            lock (s_cacheLock)
            {
                List<ISettings> settingsList = new List<ISettings>();
                LoadFromResources(settingsList);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    LoadFromAssetDatabase(settingsList);
                }
#endif

                foreach (ISettings settings in settingsList)
                {
                    s_settingsCache[settings.GetType()] = settings;
                }

                return settingsList;
            }
        }

        public static T GetSettings<T>() where T : UnityEngine.Object, ISettings
        {
            lock (s_cacheLock)
            {
                if (s_settingsCache.TryGetValue(typeof(T), out ISettings cachedSettings))
                {
                    return cachedSettings as T;
                }

                List<ISettings> allSettings = LoadAllSettings();
                T foundInCache = allSettings.OfType<T>().FirstOrDefault();
                if (foundInCache != null)
                {
                    return foundInCache;
                }

                string typeName = typeof(T).Name;
                string settingsName = typeName.EndsWith("Settings") ? typeName : typeName + "Settings";

                string[] possiblePaths = new string[]
                {
                    settingsName,
                    "Settings/" + settingsName
                };

                T settings = null;
                for (int i = 0; i < possiblePaths.Length; i++)
                {
                    string path = possiblePaths[i];
                    settings = Resources.Load<T>(path);
                    if (settings != null)
                    {
                        Debug.Log("Found settings at path: Resources/" + path);
                        break;
                    }
                }

                if (settings == null)
                {
                    T[] allResources = Resources.LoadAll<T>(string.Empty);
                    settings = allResources.FirstOrDefault();
                    if (settings != null)
                    {
                        Debug.Log("Found settings using Resources.LoadAll: " + settings.name);
                    }
                }

                if (settings != null)
                {
                    s_settingsCache[typeof(T)] = settings;
                }

                return settings;
            }
        }

        public static ISettings GetSettingsForManager(Type managerType)
        {
            lock (s_cacheLock)
            {
                return s_settingsCache.Values.FirstOrDefault(settings => settings.GetManagerType() == managerType);
            }
        }

        public static void ClearCache()
        {
            lock (s_cacheLock)
            {
                s_settingsCache.Clear();
            }
        }

        private static void LoadFromResources(List<ISettings> settingsList)
        {
            ScriptableObject[] allSettings = Resources.LoadAll<ScriptableObject>("Settings");
            foreach (ScriptableObject scriptableObject in allSettings)
            {
                if (scriptableObject is ISettings settings)
                {
                    settingsList.Add(settings);
                }
            }
        }

#if UNITY_EDITOR
        private static void LoadFromAssetDatabase(List<ISettings> settingsList)
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("/Resources/"))
                {
                    ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (asset is ISettings settings && !settingsList.Contains(settings))
                    {
                        settingsList.Add(settings);
                    }
                }
            }
        }
#endif
    }
}