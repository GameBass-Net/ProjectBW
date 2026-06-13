/// <summary>
/// Project : Easy Build System
/// Class : BuildingRendererSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers
{
    [Serializable]
    public class BuildingRendererSystem : BuildingPartSystem
    {
        [SerializeField] private List<RendererVariantData> m_variants = new List<RendererVariantData>();
        [SerializeField] private int m_activeIndex;

        public List<RendererVariantData> Variants => m_variants;

        public int ActiveIndex => m_activeIndex;

        public int Count => m_variants.Count;

        public bool DrawGizmos { get; set; }

        public RendererVariantData Active => m_variants.Count > 0 &&
            m_activeIndex >= 0 && m_activeIndex < m_variants.Count ? m_variants[m_activeIndex] : null;

        public override void Initialize(BuildingPart part)
        {
            base.Initialize(part);
            m_activeIndex = m_variants.Count > 0 ? Mathf.Clamp(m_activeIndex, 0, m_variants.Count - 1) : 0;
        }

        public virtual void BuildVariantsFromRenderers()
        {
            if (Part == null)
            {
                return;
            }

            m_variants.Clear();
            m_activeIndex = -1;

            Transform root = Part.transform;

            foreach (Transform child in root)
            {
                if (!child || ContainsVariant(child))
                {
                    continue;
                }

                RendererVariantData variant = BuildVariant(child);

                if (variant.Renderers.Length > 0 || variant.LODGroups.Length > 0)
                {
                    m_variants.Add(variant);
                }
            }

            if (m_variants.Count == 0)
            {
                RendererVariantData fallback = BuildVariant(root);
                if (fallback.Renderers.Length > 0 || fallback.LODGroups.Length > 0)
                {
                    m_variants.Add(fallback);
                }
            }

            if (m_variants.Count > 0)
            {
                SetVariant(0);
            }

            m_activeIndex = m_variants.Count > 0 ? Mathf.Clamp(m_activeIndex, 0, m_variants.Count - 1) : 0;
        }

        public virtual void AddVariantFromRoots(Transform[] roots)
        {
            if (roots == null || roots.Length == 0)
            {
                return;
            }

            for (int i = 0; i < roots.Length; i++)
            {
                Transform root = roots[i];
                if (!root || ContainsVariant(root))
                {
                    if (root)
                    {
                        Debug.LogWarning("Variant for '" + root.name + "' already exists or contains duplicate renderers.", root);
                    }

                    continue;
                }

                RendererVariantData variant = BuildVariant(root);

                if (variant.Renderers.Length > 0 || variant.LODGroups.Length > 0)
                {
                    m_variants.Add(variant);
                }
            }

            if (m_variants.Count > 0)
            {
                SetVariant(m_variants.Count - 1);
            }
        }

        public virtual void RefreshVariant(RendererVariantData variant = null)
        {
            variant = variant ?? Active;
            if (variant?.Root == null)
            {
                return;
            }

            int previousRendererCount = variant.Renderers?.Length ?? 0;

            List<Renderer> renderers = new List<Renderer>();
            List<Collider> colliders = new List<Collider>();
            List<LODGroup> lodGroups = new List<LODGroup>();
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            HashSet<Renderer> uniqueRenderers = new HashSet<Renderer>();

            foreach (Renderer renderer in variant.Root.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer || !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
                {
                    continue;
                }

                if (renderer.CompareTag("EditorOnly"))
                {
                    continue;
                }

                if (uniqueRenderers.Add(renderer))
                {
                    renderers.Add(renderer);
                }
            }

            foreach (Collider collider in variant.Root.GetComponentsInChildren<Collider>(true))
            {
                if (collider && !colliders.Contains(collider))
                {
                    colliders.Add(collider);
                }
            }

            foreach (LODGroup lodGroup in variant.Root.GetComponentsInChildren<LODGroup>(true))
            {
                if (lodGroup && !lodGroups.Contains(lodGroup))
                {
                    lodGroups.Add(lodGroup);
                }
            }

            foreach (MeshFilter meshFilter in variant.Root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (meshFilter && !meshFilters.Contains(meshFilter))
                {
                    meshFilters.Add(meshFilter);
                }
            }

            variant.Renderers = renderers.ToArray();
            variant.Colliders = colliders.ToArray();
            variant.LODGroups = lodGroups.ToArray();
            variant.MeshFilters = meshFilters.ToArray();

            if (previousRendererCount != renderers.Count && variant.MaterialVariants.Count > 1)
            {
                RebuildMaterialSetsForRendererCount(variant, previousRendererCount);
            }

            RecalculateBounds(variant);
        }

        public virtual RendererVariantData GetVariant(int index)
        {
            if (index < 0 || index >= m_variants.Count)
            {
                return null;
            }

            return m_variants[index];
        }

        public virtual void SetVariant(int index)
        {
            if (m_variants.Count == 0)
            {
                return;
            }

            int newIndex = Mathf.Clamp(index, 0, m_variants.Count - 1);

            if (newIndex == m_activeIndex)
            {
                return;
            }

            DisableAllVariants();
            m_activeIndex = newIndex;
            EnableVariant(m_activeIndex);
        }

        protected virtual void DisableAllVariants()
        {
            for (int i = 0; i < m_variants.Count; i++)
            {
                if (m_variants[i]?.Root)
                {
                    m_variants[i].Root.gameObject.SetActive(false);
                }
            }
        }

        protected virtual void EnableVariant(int index)
        {
            if (index < 0 || index >= m_variants.Count || m_variants[index]?.Root == null)
            {
                return;
            }

            m_variants[index].Root.gameObject.SetActive(true);
        }

        protected virtual void RebuildMaterialSetsForRendererCount(RendererVariantData variant, int previousRendererCount)
        {
            if (variant?.MaterialVariants == null || variant.MaterialVariants.Count == 0)
            {
                return;
            }

            for (int i = 0; i < variant.MaterialVariants.Count; i++)
            {
                MaterialSetData set = variant.MaterialVariants[i];
                if (set?.RendererMaterials == null)
                {
                    continue;
                }

                if (set.RendererMaterials.Length == previousRendererCount)
                {
                    SerializableRendererMaterials[] materials = set.RendererMaterials;
                    Array.Resize(ref materials, variant.Renderers.Length);

                    for (int j = previousRendererCount; j < materials.Length; j++)
                    {
                        materials[j] = new SerializableRendererMaterials();
                        if (variant.Renderers[j])
                        {
                            materials[j].Materials = variant.Renderers[j].sharedMaterials;
                        }
                    }

                    set.RendererMaterials = materials;
                }
            }
        }

        public virtual void RecalculateBounds(RendererVariantData variant)
        {
            if (variant?.Renderers == null)
            {
                return;
            }

            Bounds[] boundsArray = new Bounds[variant.Renderers.Length];
            bool hasValidRenderer = false;

            for (int i = 0; i < variant.Renderers.Length; i++)
            {
                Renderer renderer = variant.Renderers[i];
                if (!renderer)
                {
                    continue;
                }

                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                if (!meshFilter || !meshFilter.sharedMesh)
                {
                    continue;
                }

                hasValidRenderer = true;

                Bounds bounds = meshFilter.sharedMesh.bounds;
                Vector3 center = variant.Root.InverseTransformPoint(meshFilter.transform.TransformPoint(bounds.center));
                Vector3 extents = variant.Root.InverseTransformVector(meshFilter.transform.TransformVector(bounds.extents));

                boundsArray[i] = new Bounds(center, new Vector3(
                    Mathf.Abs(extents.x * 2f),
                    Mathf.Abs(extents.y * 2f),
                    Mathf.Abs(extents.z * 2f)));
            }

            variant.CachedBounds = boundsArray;

            if (!hasValidRenderer && m_variants != null)
            {
                int index = m_variants.IndexOf(variant);
                if (index >= 0)
                {
                    m_variants.RemoveAt(index);
                    if (m_activeIndex >= m_variants.Count)
                    {
                        m_activeIndex = m_variants.Count - 1;
                    }
                }
            }
        }

        protected virtual bool ContainsVariant(Transform root)
        {
            if (!root || m_variants.Count == 0)
            {
                return false;
            }

            Renderer[] newRenderers = root.GetComponentsInChildren<Renderer>(true);

            for (int v = 0; v < m_variants.Count; v++)
            {
                RendererVariantData variant = m_variants[v];
                if (variant == null)
                {
                    continue;
                }

                if (variant.Root == root)
                {
                    return true;
                }

                if (variant.Renderers == null)
                {
                    continue;
                }

                for (int n = 0; n < newRenderers.Length; n++)
                {
                    Renderer newRenderer = newRenderers[n];
                    if (!newRenderer)
                    {
                        continue;
                    }

                    for (int e = 0; e < variant.Renderers.Length; e++)
                    {
                        if (variant.Renderers[e] == newRenderer)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected virtual RendererVariantData BuildVariant(Transform root)
        {
            List<Renderer> renderers = new List<Renderer>();
            List<Collider> colliders = new List<Collider>();
            List<LODGroup> lodGroups = new List<LODGroup>();
            List<MeshFilter> meshFilters = new List<MeshFilter>();
            HashSet<Renderer> uniqueRenderers = new HashSet<Renderer>();

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer || !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer))
                {
                    continue;
                }

                if (renderer.CompareTag("EditorOnly"))
                {
                    continue;
                }

                if (uniqueRenderers.Add(renderer))
                {
                    renderers.Add(renderer);
                }
            }

            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (collider && !colliders.Contains(collider))
                {
                    colliders.Add(collider);
                }
            }

            foreach (LODGroup lodGroup in root.GetComponentsInChildren<LODGroup>(true))
            {
                if (lodGroup && !lodGroups.Contains(lodGroup))
                {
                    lodGroups.Add(lodGroup);
                }
            }

            foreach (MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (meshFilter && !meshFilters.Contains(meshFilter))
                {
                    meshFilters.Add(meshFilter);
                }
            }

            SerializableRendererMaterials[] initialMaterials = new SerializableRendererMaterials[renderers.Count];
            for (int i = 0; i < renderers.Count; i++)
            {
                initialMaterials[i] = new SerializableRendererMaterials();
                initialMaterials[i].Materials = renderers[i] ? renderers[i].sharedMaterials : Array.Empty<Material>();
            }

            RendererVariantData variant = new RendererVariantData
            {
                Name = root.gameObject.name,
                Root = root,
                Renderers = renderers.ToArray(),
                Colliders = colliders.ToArray(),
                LODGroups = lodGroups.ToArray(),
                MeshFilters = meshFilters.ToArray(),
                CachedBounds = new Bounds[renderers.Count],
                MaterialVariants = new List<MaterialSetData>(),
                ActiveMaterialIndex = 0
            };

            variant.MaterialVariants.Add(new MaterialSetData { RendererMaterials = initialMaterials });
            RecalculateBounds(variant);
            return variant;
        }

        public virtual void SetVariantVisibility(bool visible)
        {
            SetRenderersEnabled(visible);
        }

        public virtual void SetRenderersEnabled(bool enabled)
        {
            if (Active?.Renderers == null)
            {
                return;
            }

            for (int i = 0; i < Active.Renderers.Length; i++)
            {
                if (Active.Renderers[i])
                {
                    Active.Renderers[i].enabled = enabled;
                }
            }
        }

        public virtual void SetCollidersEnabled(bool enabled)
        {
            if (Active?.Colliders == null)
            {
                return;
            }

            for (int i = 0; i < Active.Colliders.Length; i++)
            {
                if (Active.Colliders[i])
                {
                    Active.Colliders[i].enabled = enabled;
                }
            }
        }

        public virtual void SetCollidersConvex(bool convex)
        {
            if (Active?.Colliders == null)
            {
                return;
            }

            for (int i = 0; i < Active.Colliders.Length; i++)
            {
                Collider collider = Active.Colliders[i];
                if (!collider)
                {
                    continue;
                }

                if (collider is MeshCollider meshCollider)
                {
                    meshCollider.convex = convex;
                }
                else
                {
                    Rigidbody rb = collider.attachedRigidbody;
                    if (rb)
                    {
                        rb.isKinematic = convex;
                    }
                }
            }
        }
    }
}