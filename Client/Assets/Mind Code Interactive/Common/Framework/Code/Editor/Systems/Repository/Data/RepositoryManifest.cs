/// <summary>
/// Project : Mind Code Interactive
/// Class : RepositoryManifest.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using System.Collections.Generic;

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Systems.Repository.Data
{
#if MCI
    [CreateAssetMenu(menuName = "Mind Code Interactive/Common/Repositories/Integrity Manifest")]
#endif
    public class RepositoryManifest : ScriptableObject
    {
        private static readonly Dictionary<string, RepositoryManifest> s_cache = new Dictionary<string, RepositoryManifest>();

        public string ManifestName;
        public string Version;
        public string[] RequiredLayers;
        public string[] RequiredTags;
        public string[] RequiredScriptingDefineSymbols;
        public ScriptableObject[] RequiredScriptableObjects;

        public static RepositoryManifest Get(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                return null;
            }

            if (s_cache.TryGetValue(resourceName, out RepositoryManifest cached) && cached != null)
            {
                return cached;
            }

            RepositoryManifest loaded = Resources.Load<RepositoryManifest>(resourceName);
            s_cache[resourceName] = loaded;
            return loaded;
        }

        public static void ClearCache()
        {
            s_cache.Clear();
        }
    }
}