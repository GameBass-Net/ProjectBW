/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaveSettings.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data
{
    public enum SaveProviderType { PlayerPrefs, LocalFileData, LocalFilePersistent }

    public enum SaveModeType { Manual, Automatic }

    [Serializable]
    public class BuildingSaveSettings
    {
        [SerializeField] private bool m_enableSaving = true;
        [SerializeField] private SaveModeType m_saveMode = SaveModeType.Automatic;
        [SerializeField] private bool m_autoSave = true;
        [SerializeField] private float m_autoSaveInterval = 60f;
        [SerializeField] private SaveProviderType m_saveProvider = SaveProviderType.LocalFilePersistent;

        public bool EnableSaving { get => m_enableSaving; set => m_enableSaving = value; }

        public SaveModeType SaveMode { get => m_saveMode; set => m_saveMode = value; }

        public bool AutoSave { get => m_autoSave; set => m_autoSave = value; }

        public float AutoSaveInterval { get => m_autoSaveInterval; set => m_autoSaveInterval = value; }

        public SaveProviderType SaveProvider { get => m_saveProvider; set => m_saveProvider = value; }
    }
}