/// <summary>
/// Project : Mind Code Interactive
/// Class : ReflectionCache.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils
{
    public static class ReflectionCache
    {
        private static readonly Dictionary<Type, FieldInfo> s_settingsFields = new Dictionary<Type, FieldInfo>();
        private static readonly object s_cacheLock = new object();

        public static FieldInfo GetSettingsField(Type managerType)
        {
            lock (s_cacheLock)
            {
                if (s_settingsFields.TryGetValue(managerType, out FieldInfo cachedField))
                {
                    return cachedField;
                }

                FieldInfo field = managerType.GetField("m_settings", BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    s_settingsFields[managerType] = field;
                }

                return field;
            }
        }

        public static void ClearCache()
        {
            lock (s_cacheLock)
            {
                s_settingsFields.Clear();
            }
        }
    }
}