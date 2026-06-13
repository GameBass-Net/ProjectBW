/// <summary>
/// Project : Mind Code Interactive
/// Class : BaseUniqueObject.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Interfaces;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Abstracts
{
    public abstract class BaseUniqueObject : MonoBehaviour, IUniqueObject
    {
        [SerializeField] private string m_prefabId;
        [SerializeField] private string m_uniqueId;

        private static IUniqueIdGenerator s_idGenerator = new UniqueIdGenerator();

        public string PrefabId => m_prefabId;

        public string UniqueId
        {
            get
            {
                if (string.IsNullOrEmpty(m_uniqueId))
                {
                    GenerateUniqueId();
                }

                return m_uniqueId;
            }
        }

        private static IUniqueIdGenerator IdGenerator => s_idGenerator;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(m_uniqueId) || UniqueObjectManager.UniqueExists(m_uniqueId, this))
            {
                m_uniqueId = IdGenerator.GenerateNewId();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif

        protected virtual void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (string.IsNullOrEmpty(m_uniqueId) || UniqueObjectManager.UniqueExists(m_uniqueId, this))
            {
                GenerateUniqueId();
            }

            if (string.IsNullOrEmpty(m_prefabId))
            {
                m_prefabId = System.Guid.NewGuid().ToString();
            }
        }

        public virtual void OnEnable()
        {
            if (!string.IsNullOrEmpty(m_uniqueId))
            {
                UniqueObjectManager.RegisterUnique(m_uniqueId, this);
            }

            if (!string.IsNullOrEmpty(m_prefabId))
            {
                UniqueObjectManager.RegisterPrefab(m_prefabId, this);
            }
        }

        public virtual void OnDisable()
        {
            if (!string.IsNullOrEmpty(m_uniqueId))
            {
                UniqueObjectManager.UnregisterUnique(m_uniqueId);
            }

            if (!string.IsNullOrEmpty(m_prefabId))
            {
                UniqueObjectManager.UnregisterPrefab(m_prefabId, this);
            }
        }

        public void SetUniqueId(string uniqueId)
        {
            if (!string.IsNullOrEmpty(m_uniqueId))
            {
                UniqueObjectManager.UnregisterUnique(m_uniqueId);
            }

            m_uniqueId = uniqueId;

            if (!string.IsNullOrEmpty(m_uniqueId))
            {
                UniqueObjectManager.RegisterUnique(m_uniqueId, this);
            }
        }

        public void SetPrefabId(string prefabId)
        {
            if (!string.IsNullOrEmpty(m_prefabId))
            {
                UniqueObjectManager.UnregisterPrefab(m_prefabId, this);
            }

            m_prefabId = prefabId;

            if (!string.IsNullOrEmpty(m_prefabId))
            {
                UniqueObjectManager.RegisterPrefab(m_prefabId, this);
            }
        }

        private void GenerateUniqueId()
        {
            string newId;
            do
            {
                newId = IdGenerator.GenerateNewId();
            }
            while (UniqueObjectManager.UniqueExists(newId, this));

            SetUniqueId(newId);
        }

        [ContextMenu("Generate New Unique ID")]
        private void ContextGenerateUniqueId()
        {
            GenerateUniqueId();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}