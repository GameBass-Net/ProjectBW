/// <summary>
/// Project : Mind Code Interactive
/// Class : UniqueObjectManager.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Abstracts;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject
{
    public static class UniqueObjectManager
    {
        private static readonly Dictionary<string, BaseUniqueObject> s_uniqueIds = new Dictionary<string, BaseUniqueObject>();
        private static readonly Dictionary<string, BaseUniqueObject> s_prefabIds = new Dictionary<string, BaseUniqueObject>();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingEditMode)
            {
                Clear();
            }
        }
#endif

        public static bool RegisterUnique(string id, BaseUniqueObject obj)
        {
            if (string.IsNullOrEmpty(id) || obj == null)
            {
                return false;
            }

            if (s_uniqueIds.TryGetValue(id, out BaseUniqueObject existingUnique) && existingUnique && existingUnique != obj)
            {
                return false;
            }

            s_uniqueIds[id] = obj;
            return true;
        }

        public static bool RegisterPrefab(string id, BaseUniqueObject obj)
        {
            if (string.IsNullOrEmpty(id) || obj == null)
            {
                return false;
            }

            if (s_prefabIds.TryGetValue(id, out BaseUniqueObject existingPrefab) && existingPrefab && existingPrefab != obj)
            {
                return false;
            }

            s_prefabIds[id] = obj;
            return true;
        }

        public static void UnregisterUnique(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            s_uniqueIds.Remove(id);
        }

        public static void UnregisterPrefab(string id, BaseUniqueObject obj)
        {
            if (string.IsNullOrEmpty(id) || obj == null)
            {
                return;
            }

            if (s_prefabIds.TryGetValue(id, out BaseUniqueObject existingPrefab) && existingPrefab == obj)
            {
                s_prefabIds.Remove(id);
            }
        }

        public static bool UniqueExists(string id, BaseUniqueObject self)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            return s_uniqueIds.TryGetValue(id, out BaseUniqueObject existingUnique) && existingUnique && existingUnique != self;
        }

        public static BaseUniqueObject GetByUniqueId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            s_uniqueIds.TryGetValue(id, out BaseUniqueObject obj);
            return obj;
        }

        public static T GetByUniqueId<T>(string id) where T : BaseUniqueObject => GetByUniqueId(id) as T;

        public static BaseUniqueObject GetByPrefabId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            s_prefabIds.TryGetValue(id, out BaseUniqueObject obj);
            return obj;
        }

        public static T GetByPrefabId<T>(string id) where T : BaseUniqueObject => GetByPrefabId(id) as T;

        public static bool TryGetByUniqueId(string id, out BaseUniqueObject obj)
        {
            obj = GetByUniqueId(id);
            return obj != null;
        }

        public static bool TryGetByPrefabId(string id, out BaseUniqueObject obj)
        {
            obj = GetByPrefabId(id);
            return obj != null;
        }

        public static void Clear()
        {
            s_uniqueIds.Clear();
            s_prefabIds.Clear();
        }
    }
}