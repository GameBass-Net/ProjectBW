/// <summary>
/// Project : Mind Code Interactive
/// Class : Manager.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Constants;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Interfaces;
using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Utils;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.ManagerSystem.Abstracts
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_HIGH_PRIORITY)]
    public abstract class Manager<TSettings> : MonoBehaviour, IManager
        where TSettings : Settings<TSettings>
    {
        [SerializeField] protected TSettings m_settings;

        private bool m_isInitialized;

        public TSettings Settings => m_settings;
        public string Name => m_settings != null ? m_settings.ManagerName : GetType().Name;
        public bool IsInitialized => m_isInitialized;

        public virtual void Initialize()
        {
            if (m_isInitialized)
            {
                return;
            }

            try
            {
                if (m_settings == null)
                {
                    m_settings = LoadSettings();
                }

                ValidateSettings();
                OnInitialize();
                m_isInitialized = true;
            }
            catch (Exception exception)
            {
                Debug.LogError("[" + Name + "] Failed to initialize: " + exception.Message);
                throw;
            }
        }

        public virtual void Shutdown()
        {
            if (!m_isInitialized)
            {
                return;
            }

            try
            {
                OnShutdown();
                m_isInitialized = false;
            }
            catch (Exception exception)
            {
                Debug.LogError("[" + Name + "] Failed to shutdown: " + exception.Message);
            }
        }

        public static TManagerSettings GetSettings<TManagerSettings>()
            where TManagerSettings : Settings<TManagerSettings>
        {
            return ManagerLocator.GetSettings<Manager<TManagerSettings>, TManagerSettings>();
        }

        protected virtual void Awake() => ManagerLocator.RegisterManager(this);

        protected virtual void OnDestroy()
        {
            if (m_isInitialized)
            {
                Shutdown();
            }

            ManagerLocator.UnregisterManager(this);
        }

        protected virtual void OnValidate()
        {
            if (m_settings != null && m_isInitialized)
            {
                try
                {
                    ValidateSettings();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("[" + Name + "] Settings validation failed: " + exception.Message);
                }
            }
        }

        protected abstract void OnInitialize();

        protected virtual void OnShutdown() { }

        protected virtual void ValidateSettings()
        {
            if (m_settings == null)
            {
                throw new InvalidOperationException("Settings are required for " + GetType().Name);
            }
        }

        protected virtual TSettings LoadSettings() => SettingsLoader.GetSettings<TSettings>();
    }
}