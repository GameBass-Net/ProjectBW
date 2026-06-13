/// <summary>
/// Project : Easy Build System
/// Class : BuildingTerrainSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Terrain
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Terrain.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Managers.Implementations.Terrain
{
    public class BuildingTerrainSystem : BuildingManagerSubSystem
    {
        private Dictionary<UnityEngine.Terrain, BuildingTerrainBackupData> m_terrainBackups =
            new Dictionary<UnityEngine.Terrain, BuildingTerrainBackupData>();

        public UnityEvent<UnityEngine.Terrain> OnAfterBackup;

        public UnityEvent<UnityEngine.Terrain> OnAfterRestore;

        public override void Initialize()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BackupAll();
        }

        public override void Shutdown()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            RestoreAll();
        }

        public virtual void Backup(UnityEngine.Terrain terrain)
        {
            if (terrain == null)
            {
                throw new ArgumentNullException(nameof(terrain));
            }

            if (m_terrainBackups.ContainsKey(terrain))
            {
                return;
            }

            m_terrainBackups.Add(terrain, new BuildingTerrainBackupData(terrain));
            OnAfterBackup?.Invoke(terrain);
        }

        public virtual void Restore(UnityEngine.Terrain terrain)
        {
            if (terrain == null)
            {
                return;
            }

            if (!m_terrainBackups.TryGetValue(terrain, out BuildingTerrainBackupData backupData))
            {
                return;
            }

            TerrainData terrainData = terrain.terrainData;
            terrainData.SetHeights(0, 0, backupData.Heights);
            terrainData.SetAlphamaps(0, 0, backupData.AlphaMaps);

            foreach (KeyValuePair<int, int[,]> detailLayer in backupData.DetailLayers)
            {
                terrainData.SetDetailLayer(0, 0, detailLayer.Key, detailLayer.Value);
            }

            terrainData.treeInstances = backupData.TreeInstances;
            terrainData.treePrototypes = backupData.TreePrototypes;
            terrainData.detailPrototypes = backupData.DetailPrototypes;
            OnAfterRestore?.Invoke(terrain);
        }

        public virtual void BackupAll()
        {
#if UNITY_2023_1_OR_NEWER
#pragma warning disable CS0618
            UnityEngine.Terrain[] terrains =
                UnityEngine.Object.FindObjectsByType<UnityEngine.Terrain>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#pragma warning restore CS0618
#else
            UnityEngine.Terrain[] terrains =
                UnityEngine.Object.FindObjectsOfType<UnityEngine.Terrain>();
#endif

            for (int i = 0; i < terrains.Length; i++)
            {
                UnityEngine.Terrain t = terrains[i];
                if (t)
                {
                    Backup(t);
                }
            }
        }

        public virtual void RestoreAll()
        {
            foreach (KeyValuePair<UnityEngine.Terrain, BuildingTerrainBackupData> terrainBackupPair in m_terrainBackups)
            {
                Restore(terrainBackupPair.Key);
            }
        }
    }
}