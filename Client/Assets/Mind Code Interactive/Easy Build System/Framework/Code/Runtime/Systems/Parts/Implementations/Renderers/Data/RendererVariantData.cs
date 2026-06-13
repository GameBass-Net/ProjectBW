/// <summary>
/// Project : Easy Build System
/// Class : RendererVariantData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data
{
    [Serializable]
    public sealed class RendererVariantData
    {
        [SerializeField] private string m_name;
        [SerializeField] private Transform m_root;
        [SerializeField] private Renderer[] m_renderers;
        [SerializeField] private Collider[] m_colliders;
        [SerializeField] private LODGroup[] m_lodGroups;
        [SerializeField] private MeshFilter[] m_meshFilters;
        [SerializeField] private Bounds[] m_cachedBounds;
        [SerializeField] private List<MaterialSetData> m_materialVariants = new List<MaterialSetData>();
        [SerializeField] private int m_activeMaterialIndex;

        public string Name { get => m_name; set => m_name = value; }

        public Transform Root { get => m_root; set => m_root = value; }

        public Renderer[] Renderers { get => m_renderers; set => m_renderers = value; }

        public Collider[] Colliders { get => m_colliders; set => m_colliders = value; }

        public LODGroup[] LODGroups { get => m_lodGroups; set => m_lodGroups = value; }

        public MeshFilter[] MeshFilters { get => m_meshFilters; set => m_meshFilters = value; }

        public Bounds[] CachedBounds { get => m_cachedBounds; set => m_cachedBounds = value; }

        public List<MaterialSetData> MaterialVariants { get => m_materialVariants; set => m_materialVariants = value; }

        public int ActiveMaterialIndex { get => m_activeMaterialIndex; set => m_activeMaterialIndex = value; }

        public Bounds GetLocalBounds()
        {
            Bounds combinedBounds = new Bounds();
            bool isFirstBound = true;

            if (m_cachedBounds != null && m_cachedBounds.Length == (m_renderers?.Length ?? 0))
            {
                for (int i = 0; i < m_cachedBounds.Length; i++)
                {
                    if (m_renderers[i] == null)
                    {
                        continue;
                    }

                    if (isFirstBound)
                    {
                        combinedBounds = m_cachedBounds[i];
                        isFirstBound = false;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(m_cachedBounds[i]);
                    }
                }
            }
            else if (m_renderers != null && m_root != null)
            {
                for (int i = 0; i < m_renderers.Length; i++)
                {
                    Renderer renderer = m_renderers[i];
                    if (!renderer)
                    {
                        continue;
                    }

                    Bounds worldBounds = renderer.bounds;
                    Vector3 localCenter = m_root.parent != null ? m_root.parent.InverseTransformPoint(worldBounds.center) : worldBounds.center;
                    Vector3 localExtents = m_root.parent != null ? m_root.parent.InverseTransformVector(worldBounds.extents) : worldBounds.extents;
                    Bounds localBounds = new Bounds(localCenter, localExtents * 2f);

                    if (isFirstBound)
                    {
                        combinedBounds = localBounds;
                        isFirstBound = false;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(localBounds);
                    }
                }
            }

            return combinedBounds.RoundBounds(3);
        }

        public Bounds GetWorldBounds()
        {
            Bounds localBounds = GetLocalBounds();
            Bounds worldBounds = m_root != null ? localBounds.ToWorldBounds(m_root) : localBounds;
            return worldBounds.RoundBounds(3);
        }

        public void SetMaterialSet(int setIndex)
        {
            if (setIndex < 0 || setIndex >= m_materialVariants.Count)
            {
                return;
            }

            m_activeMaterialIndex = setIndex;

            MaterialSetData activeSet = m_materialVariants[setIndex];
            if (activeSet.RendererMaterials == null || activeSet.RendererMaterials.Length == 0)
            {
                return;
            }

            for (int i = 0; i < m_renderers.Length && i < activeSet.RendererMaterials.Length; i++)
            {
                if (m_renderers[i] && activeSet.RendererMaterials[i]?.Materials != null)
                {
                    m_renderers[i].sharedMaterials = activeSet.RendererMaterials[i].Materials;
                    m_renderers[i].SetPropertyBlock(null);
                }
            }
        }

        public void AddMaterialSet()
        {
            MaterialSetData newSet = new MaterialSetData { RendererMaterials = CollectCurrentMaterials() };
            m_materialVariants.Add(newSet);
        }

        public void RemoveMaterialSet(int index)
        {
            if (index <= 0 || index >= m_materialVariants.Count)
            {
                return;
            }

            m_materialVariants.RemoveAt(index);

            if (m_activeMaterialIndex >= m_materialVariants.Count)
            {
                m_activeMaterialIndex = Mathf.Max(0, m_materialVariants.Count - 1);
                SetMaterialSet(m_activeMaterialIndex);
            }
        }

        private SerializableRendererMaterials[] CollectCurrentMaterials()
        {
            SerializableRendererMaterials[] materials = new SerializableRendererMaterials[m_renderers.Length];

            for (int i = 0; i < m_renderers.Length; i++)
            {
                materials[i] = new SerializableRendererMaterials();

                if (m_renderers[i])
                {
                    materials[i].Materials = m_renderers[i].sharedMaterials;
                }
                else
                {
                    materials[i].Materials = Array.Empty<Material>();
                }
            }

            return materials;
        }
    }
}