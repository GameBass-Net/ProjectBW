/// <summary>
/// Project : Easy Build System
/// Class : BuildingSaveSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save
/// Copyright :  2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.EventSystem;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save.Events;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Networking.Interfaces;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Save
{
    public class BuildingSaveSystem : BuildingManagerSubSystem
    {
        protected const string SAVE_KEY_PREFIX = "EBS_Save";
        protected const string META_KEY_PREFIX = "EBS_Meta";

        protected static INetworkBuildingSaveAdapter s_saveAdapter;

        protected BuildingSaveSettings m_settings;
        protected float m_autoSaveTimer;
        protected bool m_isLoading;

        protected readonly HashSet<BuildingPart> m_saveableBuffer = new HashSet<BuildingPart>();
        protected readonly List<BuildingPartData> m_partsDataBuffer = new List<BuildingPartData>();
        protected readonly HashSet<string> m_prefabIdsBuffer = new HashSet<string>();
        protected readonly List<string> m_prefabIdsListBuffer = new List<string>();
        protected readonly HashSet<BuildingPart> m_loadedParts = new HashSet<BuildingPart>();

        public BuildingSaveSettings Settings => m_settings;
        public bool IsLoading => m_isLoading;

        public BuildingSaveSystem(BuildingManager manager, BuildingSaveSettings settings)
        {
            m_manager = manager;
            m_settings = settings;
        }

        public override void Initialize()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_settings.SaveMode == SaveModeType.Automatic)
            {
                LoadBuildings();
            }
        }

        public override void Shutdown()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_settings.SaveMode == SaveModeType.Automatic)
            {
                SaveBuildings();
            }

            m_loadedParts.Clear();
        }

        public override void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (!m_settings.EnableSaving || m_settings.SaveMode != SaveModeType.Automatic || !m_settings.AutoSave)
            {
                return;
            }

            m_autoSaveTimer += Time.deltaTime;

            if (m_autoSaveTimer >= m_settings.AutoSaveInterval)
            {
                m_autoSaveTimer = 0f;
                SaveBuildings();
            }
        }

        public virtual void SaveBuildings()
        {
            if (!CanSave())
            {
                return;
            }

            try
            {
                BuildingSaveData saveData = CreateSaveData();
                BuildingSaveMetaData metaData = CreateMetaData(saveData);

                EventPublisher.Publish(new BuildingSaveEvent.SaveStartedEventArgs(saveData));

                SaveToStorage(saveData, metaData);

                if (Application.isPlaying)
                {
                    EventPublisher.Publish(new BuildingSaveEvent.SaveCompletedEventArgs(m_saveableBuffer));
                }

                if (saveData.BuildingCount > 0)
                {
                    Debug.Log($"Successfully saved {saveData.BuildingCount} building(s).");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save buildings: {exception.Message}");
            }
        }

        public virtual void LoadBuildings()
        {
            if (m_isLoading || !CanLoad())
            {
                return;
            }

            try
            {
                m_isLoading = true;
                s_saveAdapter?.OnBeforeLoad();

                float startTime = Application.isPlaying ? Time.realtimeSinceStartup : 0f;

                (BuildingSaveData saveData, BuildingSaveMetaData metaData) = LoadFromStorage();

                if (saveData == null || !ValidateData(saveData, metaData))
                {
                    return;
                }

                EventPublisher.Publish(new BuildingSaveEvent.LoadStartedEventArgs(saveData));

                DestroyPreviouslyLoadedParts();

                HashSet<BuildingPart> spawnedParts = SpawnBuildingParts(saveData);

                m_loadedParts.Clear();
                foreach (BuildingPart spawned in spawnedParts)
                {
                    if (spawned != null)
                    {
                        m_loadedParts.Add(spawned);
                    }
                }

                float loadTime = Application.isPlaying ? Time.realtimeSinceStartup - startTime : 0f;

                if (Application.isPlaying)
                {
                    EventPublisher.Publish(new BuildingSaveEvent.LoadCompletedEventArgs(spawnedParts, loadTime));
                }

                s_saveAdapter?.OnAfterLoad();

                if (saveData.BuildingCount > 0)
                {
                    Debug.Log($"Successfully loaded {saveData.BuildingCount} building(s) in {loadTime:F3}s");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load buildings: {exception.Message}");
            }
            finally
            {
                m_isLoading = false;
            }
        }

        public virtual void DeleteSave()
        {
            try
            {
                if (m_settings.SaveProvider == SaveProviderType.PlayerPrefs)
                {
                    PlayerPrefs.DeleteKey(GetSaveKey());
                    PlayerPrefs.DeleteKey(GetMetaKey());
                    PlayerPrefs.Save();
                }
                else
                {
                    DeleteFileIfExists(GetSaveFilePath());
                    DeleteFileIfExists(GetMetaFilePath());
                }

                Debug.Log("Save data deleted successfully.");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to delete save data: {exception.Message}");
            }
        }

        public virtual bool HasSaveData()
        {
            return m_settings.SaveProvider == SaveProviderType.PlayerPrefs
                ? PlayerPrefs.HasKey(GetSaveKey())
                : File.Exists(GetSaveFilePath());
        }

        public virtual BuildingSaveMetaData GetSaveMetaData()
        {
            (_, BuildingSaveMetaData metaData) = LoadFromStorage();
            return metaData;
        }

        public static void SetNetworkSaveAdapter(INetworkBuildingSaveAdapter adapter)
        {
            s_saveAdapter = adapter;
        }

        private bool CanSave()
        {
            if (!m_settings.EnableSaving)
            {
                return false;
            }

            if (s_saveAdapter != null && !s_saveAdapter.IsAuthority)
            {
                Debug.LogWarning("Only server/master can save buildings in network mode.");
                return false;
            }

            return s_saveAdapter == null || s_saveAdapter.ShouldSaveLocally;
        }

        private bool CanLoad()
        {
            if (!m_settings.EnableSaving)
            {
                return false;
            }

            if (s_saveAdapter != null && !s_saveAdapter.IsAuthority)
            {
                Debug.LogWarning("Only server/master can load buildings in network mode.");
                return false;
            }

            return true;
        }

        protected virtual void SaveToStorage(BuildingSaveData saveData, BuildingSaveMetaData metaData)
        {
            string saveJson = JsonUtility.ToJson(saveData, false);
            string metaJson = JsonUtility.ToJson(metaData, false);

            if (m_settings.SaveProvider == SaveProviderType.PlayerPrefs)
            {
                PlayerPrefs.SetString(GetSaveKey(), saveJson);
                PlayerPrefs.SetString(GetMetaKey(), metaJson);
                PlayerPrefs.Save();
                return;
            }

            string savePath = GetSaveFilePath();
            string metaPath = GetMetaFilePath();
            CreateDirectoryIfNeeded(Path.GetDirectoryName(savePath));

            string backupSave = savePath + ".bak";
            string backupMeta = metaPath + ".bak";

            if (File.Exists(savePath))
            {
                File.Copy(savePath, backupSave, true);
            }

            if (File.Exists(metaPath))
            {
                File.Copy(metaPath, backupMeta, true);
            }

            File.WriteAllText(savePath, saveJson);
            File.WriteAllText(metaPath, metaJson);
        }

        protected virtual (BuildingSaveData, BuildingSaveMetaData) LoadFromStorage()
        {
            string saveJson;
            string metaJson;

            if (m_settings.SaveProvider == SaveProviderType.PlayerPrefs)
            {
                saveJson = PlayerPrefs.GetString(GetSaveKey(), string.Empty);
                metaJson = PlayerPrefs.GetString(GetMetaKey(), string.Empty);
            }
            else
            {
                string savePath = GetSaveFilePath();
                if (!File.Exists(savePath))
                {
                    return (null, null);
                }

                string metaPath = GetMetaFilePath();
                saveJson = File.ReadAllText(savePath);
                metaJson = File.Exists(metaPath) ? File.ReadAllText(metaPath) : string.Empty;
            }

            if (string.IsNullOrEmpty(saveJson) || string.IsNullOrEmpty(metaJson))
            {
                return (null, null);
            }

            return (
                JsonUtility.FromJson<BuildingSaveData>(saveJson),
                JsonUtility.FromJson<BuildingSaveMetaData>(metaJson)
            );
        }

        protected virtual BuildingSaveData CreateSaveData()
        {
            CollectSaveableBuildingParts();

            m_partsDataBuffer.Clear();

            foreach (BuildingPart part in m_saveableBuffer)
            {
                if (part.GetSaveData() is BuildingPartData data)
                {
                    m_partsDataBuffer.Add(data);
                }
            }

            return new BuildingSaveData(
                DateTime.Now,
                SceneManager.GetActiveScene().name,
                m_partsDataBuffer.Count,
                new List<BuildingPartData>(m_partsDataBuffer));
        }

        protected virtual BuildingSaveMetaData CreateMetaData(BuildingSaveData saveData)
        {
            m_prefabIdsBuffer.Clear();

            for (int i = 0; i < saveData.BuildingData.Count; i++)
            {
                m_prefabIdsBuffer.Add(saveData.BuildingData[i].PrefabId);
            }

            m_prefabIdsListBuffer.Clear();
            foreach (string id in m_prefabIdsBuffer)
            {
                m_prefabIdsListBuffer.Add(id);
            }

            return new BuildingSaveMetaData(
                hasData: true,
                sceneName: SceneManager.GetActiveScene().name,
                saveTime: DateTime.Now,
                saverVersion: Application.version,
                buildingCount: saveData.BuildingCount,
                usedPrefabIds: new List<string>(m_prefabIdsListBuffer));
        }

        protected virtual HashSet<BuildingPart> SpawnBuildingParts(BuildingSaveData saveData)
        {
            HashSet<BuildingPart> loadedParts = new HashSet<BuildingPart>();
            HashSet<string> loadedIds = new HashSet<string>();

            if (saveData.BuildingData == null)
            {
                return loadedParts;
            }

            for (int i = 0; i < saveData.BuildingData.Count; i++)
            {
                BuildingPartData partData = saveData.BuildingData[i];
                BuildingPart existing = m_manager.GetPartByUniqueId(partData.UniqueId);

                if (existing != null)
                {
                    existing.LoadSaveData(partData);
                    loadedParts.Add(existing);
                    loadedIds.Add(partData.UniqueId);
                }
            }

            if (s_saveAdapter != null && s_saveAdapter.IsAuthority)
            {
                for (int i = 0; i < saveData.BuildingData.Count; i++)
                {
                    BuildingPartData partData = saveData.BuildingData[i];
                    if (!loadedIds.Contains(partData.UniqueId))
                    {
                        s_saveAdapter.SpawnLoadedBuilding(partData);
                    }
                }

                return loadedParts;
            }

            for (int i = 0; i < saveData.BuildingData.Count; i++)
            {
                BuildingPartData partData = saveData.BuildingData[i];
                if (loadedIds.Contains(partData.UniqueId))
                {
                    continue;
                }

                BuildingPart prefab = m_manager.GetPartByPrefabId(partData.PrefabId);

                if (prefab == null)
                {
                    Debug.LogError($"Building part prefab '{partData.PrefabId}' not found");
                    continue;
                }

                BuildingSocket socket = partData.HasAttachedSocket
                    ? BuildingSocket.GetSocketById(partData.AttachedSocketId)
                    : null;

                BuildingPart instance = m_manager.PlacePart(
                    prefab,
                    partData.Position,
                    partData.Rotation,
                    partData.Scale,
                    socket);

                instance?.LoadSaveData(partData);

                if (instance != null)
                {
                    loadedParts.Add(instance);
                }
            }

            return loadedParts;
        }

        protected virtual bool ValidateData(BuildingSaveData saveData, BuildingSaveMetaData metaData)
        {
            if (saveData?.BuildingData == null || metaData == null)
            {
                return false;
            }

            if (metaData.SceneName != SceneManager.GetActiveScene().name)
            {
                return false;
            }

            for (int i = 0; i < metaData.UsedPrefabIds.Count; i++)
            {
                if (m_manager.GetPartByPrefabId(metaData.UsedPrefabIds[i]) == null)
                {
                    Debug.LogError($"Required Building Part '{metaData.UsedPrefabIds[i]}' not found in BuildingManager");
                    return false;
                }
            }

            return true;
        }

        protected virtual void CollectSaveableBuildingParts()
        {
            m_saveableBuffer.Clear();

            foreach (BuildingPart part in m_manager.GetRegisteredParts)
            {
                if (part == null)
                {
                    continue;
                }

                BuildingPart.BuildingState state = part.State;
                if (state != BuildingPart.BuildingState.Placed && state != BuildingPart.BuildingState.Queue)
                {
                    continue;
                }

                if (part.GetSaveData() == null)
                {
                    continue;
                }

                m_saveableBuffer.Add(part);
            }
        }

        protected virtual void DestroyPreviouslyLoadedParts()
        {
            if (m_loadedParts.Count == 0)
            {
                return;
            }

            foreach (BuildingPart part in m_loadedParts)
            {
                if (part == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(part.gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(part.gameObject);
                }
            }

            m_loadedParts.Clear();
        }

        protected virtual string GetSaveFilePath()
        {
            return Path.Combine(GetBasePath(), GetSaveFileName(".json"));
        }

        protected virtual string GetMetaFilePath()
        {
            return Path.Combine(GetBasePath(), GetSaveFileName(".meta"));
        }

        protected virtual string GetSaveKey() => $"{SAVE_KEY_PREFIX}_{SceneManager.GetActiveScene().name}";

        protected virtual string GetMetaKey() => $"{META_KEY_PREFIX}_{SceneManager.GetActiveScene().name}";

        protected virtual string GetSaveFileName(string extension)
        {
            return $"building_save_{SceneManager.GetActiveScene().name}{extension}";
        }

        protected virtual string GetBasePath()
        {
            if (Application.isPlaying && m_settings.SaveProvider == SaveProviderType.LocalFilePersistent)
            {
                return Application.persistentDataPath;
            }

            return m_settings.SaveProvider == SaveProviderType.LocalFileData
                ? Application.dataPath
                : Application.persistentDataPath;
        }

        private static void CreateDirectoryIfNeeded(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}