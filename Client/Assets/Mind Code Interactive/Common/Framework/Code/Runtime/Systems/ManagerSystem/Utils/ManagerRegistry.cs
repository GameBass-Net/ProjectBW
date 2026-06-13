/// <summary>
/// Project : Mind Code Interactive
/// Class : ManagerRegistry.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils
{
    public class ManagerRegistry
    {
        private readonly Dictionary<Type, IManager> m_managers = new Dictionary<Type, IManager>();
        private readonly object m_registryLock = new object();

        public IReadOnlyDictionary<Type, IManager> Managers => m_managers;

        public void RegisterManager(IManager manager)
        {
            if (manager == null)
            {
                return;
            }

            lock (m_registryLock)
            {
                Type managerType = manager.GetType();

                if (m_managers.ContainsKey(managerType))
                {
                    return;
                }

                m_managers[managerType] = manager;
            }
        }

        public void UnregisterManager(IManager manager)
        {
            if (manager == null)
            {
                return;
            }

            lock (m_registryLock)
            {
                m_managers.Remove(manager.GetType());
            }
        }

        public T GetManager<T>() where T : class, IManager
        {
            lock (m_registryLock)
            {
                m_managers.TryGetValue(typeof(T), out IManager manager);
                return manager as T;
            }
        }

        public bool HasManager(Type type)
        {
            lock (m_registryLock)
            {
                return m_managers.ContainsKey(type);
            }
        }

        public List<IManager> GetAllManagers()
        {
            lock (m_registryLock)
            {
                return m_managers.Values.ToList();
            }
        }

        public void Clear()
        {
            lock (m_registryLock)
            {
                m_managers.Clear();
            }
        }
    }
}