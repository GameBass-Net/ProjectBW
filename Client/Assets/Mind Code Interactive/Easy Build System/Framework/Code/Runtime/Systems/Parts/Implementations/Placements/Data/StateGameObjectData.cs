/// <summary>
/// Project : Easy Build System
/// Class : StateGameObjectData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data
{
    [Serializable]
    public class StateGameObjectData
    {
        [SerializeField] private BuildingPart.BuildingState m_state;
        [SerializeField] private GameObject[] m_gameObjectsToDisable;
        [SerializeField] private GameObject[] m_gameObjectsToEnable;

        public BuildingPart.BuildingState State { get => m_state; set => m_state = value; }

        public GameObject[] GameObjectsToDisable { get => m_gameObjectsToDisable; set => m_gameObjectsToDisable = value; }

        public GameObject[] GameObjectsToEnable { get => m_gameObjectsToEnable; set => m_gameObjectsToEnable = value; }

        public void Apply()
        {
            for (int i = 0; i < (m_gameObjectsToDisable?.Length ?? 0); i++)
            {
                if (m_gameObjectsToDisable[i])
                {
                    m_gameObjectsToDisable[i].SetActive(false);
                }
            }

            for (int i = 0; i < (m_gameObjectsToEnable?.Length ?? 0); i++)
            {
                if (m_gameObjectsToEnable[i])
                {
                    m_gameObjectsToEnable[i].SetActive(true);
                }
            }
        }
    }
}