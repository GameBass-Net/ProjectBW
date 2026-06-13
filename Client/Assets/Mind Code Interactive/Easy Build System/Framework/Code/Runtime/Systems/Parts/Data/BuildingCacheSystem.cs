/// <summary>
/// Project : Easy Build System
/// Class : BuildingCacheSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
{
    [Serializable]
    public sealed class BuildingCacheSystem : BuildingPartSystem
    {
        [SerializeField] private BuildingSocket[] m_cachedSockets = Array.Empty<BuildingSocket>();
        [SerializeField] private Rigidbody[] m_cachedRigidbodies = Array.Empty<Rigidbody>();

        private Material[][] m_originalMaterials;
        private bool[] m_originalKinematicStates = Array.Empty<bool>();
        private MaterialPropertyBlock m_cachedMaterialPropertyBlock;
        private Dictionary<int, Material[][]> m_cachedPreviewMaterials = new Dictionary<int, Material[][]>();

        public BuildingSocket[] Sockets => m_cachedSockets;

        public Rigidbody[] Rigidbodies => m_cachedRigidbodies;

        public override void Initialize(BuildingPart part)
        {
            base.Initialize(part);

            m_cachedSockets = part.GetComponentsInChildren<BuildingSocket>(true);

            m_cachedRigidbodies = part.GetComponentsInChildren<Rigidbody>(true);
            m_originalKinematicStates = new bool[m_cachedRigidbodies.Length];
            for (int i = 0; i < m_cachedRigidbodies.Length; i++)
            {
                m_originalKinematicStates[i] = m_cachedRigidbodies[i].isKinematic;
            }

            m_cachedMaterialPropertyBlock = new MaterialPropertyBlock();
            CaptureOriginalMaterials();
        }

        public override void Shutdown()
        {
            ClearPreviewMaterialCache();
            m_cachedMaterialPropertyBlock = null;
            m_originalMaterials = null;
            base.Shutdown();
        }

        public void Refresh()
        {
            m_cachedSockets = m_part.GetComponentsInChildren<BuildingSocket>(true);
            ClearPreviewMaterialCache();
            m_cachedMaterialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetRigidbodiesKinematic(bool kinematic)
        {
            if (m_cachedRigidbodies == null)
            {
                return;
            }

            for (int i = 0; i < m_cachedRigidbodies.Length; i++)
            {
                Rigidbody rb = m_cachedRigidbodies[i];
                if (rb)
                {
                    rb.isKinematic = kinematic;
                }
            }
        }

        public void RestoreRigidbodiesKinematic()
        {
            if (m_cachedRigidbodies == null || m_originalKinematicStates == null)
            {
                return;
            }

            for (int i = 0; i < m_cachedRigidbodies.Length; i++)
            {
                Rigidbody rb = m_cachedRigidbodies[i];
                if (rb)
                {
                    rb.isKinematic = m_originalKinematicStates[i];
                }
            }
        }

        public void SetSocketsEnabled(bool enabled)
        {
            for (int i = 0; i < m_cachedSockets.Length; i++)
            {
                if (m_cachedSockets[i])
                {
                    m_cachedSockets[i].enabled = enabled;
                }
            }
        }

        public MaterialPropertyBlock GetMaterialPropertyBlock()
        {
            if (m_cachedMaterialPropertyBlock == null)
            {
                m_cachedMaterialPropertyBlock = new MaterialPropertyBlock();
            }
            return m_cachedMaterialPropertyBlock;
        }

        public Material[] GetOriginalMaterialsForRenderer(int rendererIndex)
        {
            if (m_originalMaterials == null || rendererIndex < 0 || rendererIndex >= m_originalMaterials.Length)
            {
                return null;
            }

            return m_originalMaterials[rendererIndex];
        }

        public Material[][] GetOrCreatePreviewMaterials(int stateHash, Material customMaterial = null)
        {
            if (m_cachedPreviewMaterials.TryGetValue(stateHash, out Material[][] cachedMaterials))
            {
                return cachedMaterials;
            }

            Renderer[] renderers = m_part.RendererSystem.Active?.Renderers;
            if (renderers == null)
            {
                return null;
            }

            Material[][] previewMaterials = new Material[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!renderer)
                {
                    continue;
                }

                Material[] sourceMaterials = renderer.sharedMaterials;
                if (sourceMaterials == null || sourceMaterials.Length == 0)
                {
                    continue;
                }

                Material[] previewArray = new Material[sourceMaterials.Length];
                for (int m = 0; m < sourceMaterials.Length; m++)
                {
                    Material templateMaterial = customMaterial != null ? customMaterial : sourceMaterials[m];
                    previewArray[m] = new Material(templateMaterial);
                }

                previewMaterials[i] = previewArray;
            }

            m_cachedPreviewMaterials[stateHash] = previewMaterials;
            return previewMaterials;
        }

        private void CaptureOriginalMaterials()
        {
            Renderer[] renderers = m_part.RendererSystem.Active?.Renderers;
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            m_originalMaterials = new Material[renderers.Length][];

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    m_originalMaterials[i] = Array.Empty<Material>();
                    continue;
                }

                Material[] sourceMaterials = renderers[i].sharedMaterials;
                if (sourceMaterials != null && sourceMaterials.Length > 0)
                {
                    m_originalMaterials[i] = new Material[sourceMaterials.Length];
                    for (int m = 0; m < sourceMaterials.Length; m++)
                    {
                        m_originalMaterials[i][m] = sourceMaterials[m];
                    }
                }
                else
                {
                    m_originalMaterials[i] = Array.Empty<Material>();
                }
            }
        }

        public void RestoreOriginalMaterials()
        {
            if (m_originalMaterials == null)
            {
                return;
            }

            Renderer[] renderers = m_part.RendererSystem.Active?.Renderers;
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length && i < m_originalMaterials.Length; i++)
            {
                if (renderers[i] != null && m_originalMaterials[i] != null)
                {
                    renderers[i].sharedMaterials = m_originalMaterials[i];
                    renderers[i].SetPropertyBlock(null);
                }
            }
        }

        private void ClearPreviewMaterialCache()
        {
            foreach (KeyValuePair<int, Material[][]> entry in m_cachedPreviewMaterials)
            {
                Material[][] materials = entry.Value;
                if (materials != null)
                {
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] != null)
                        {
                            for (int m = 0; m < materials[i].Length; m++)
                            {
                                if (materials[i][m] != null)
                                {
                                    if (Application.isPlaying)
                                    {
                                        UnityEngine.Object.Destroy(materials[i][m]);
                                    }
                                    else
                                    {
                                        UnityEngine.Object.DestroyImmediate(materials[i][m]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            m_cachedPreviewMaterials.Clear();
        }
    }
}