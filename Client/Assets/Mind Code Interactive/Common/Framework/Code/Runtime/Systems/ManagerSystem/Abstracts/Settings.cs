/// <summary>
/// Project : Mind Code Interactive
/// Class : Settings.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Abstracts
{
    public abstract class Settings<T> : ScriptableObject, ISettings
        where T : Settings<T>
    {
        [Header("Manager Configuration")]
        [SerializeField] private string m_managerName;
        [SerializeField, Range(-100, 100)] private int m_priority;
        [SerializeField] private bool m_autoInitialize = true;
        [SerializeField] private bool m_autoCreate = true;

        public string ManagerName
        {
            get
            {
                if (string.IsNullOrEmpty(m_managerName))
                {
                    return GetType().Name.Replace("Settings", string.Empty);
                }

                return m_managerName;
            }
        }

        public int Priority => m_priority;
        public bool AutoInitialize => m_autoInitialize;
        public bool AutoCreate => m_autoCreate;

        public abstract Type GetManagerType();

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(m_managerName))
            {
                m_managerName = GetType().Name.Replace("Settings", string.Empty);
            }

            m_priority = Mathf.Clamp(m_priority, -100, 100);
        }

        protected virtual void Reset()
        {
            m_managerName = GetType().Name.Replace("Settings", string.Empty);
            m_priority = 0;
            m_autoInitialize = true;
            m_autoCreate = true;
        }
    }
}