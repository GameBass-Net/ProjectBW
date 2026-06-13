/// <summary>
/// Project : Easy Build System
/// Class : BuildingPlacementSystem.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Extensions;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Renderers.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements
{
    [Serializable]
    public class BuildingPlacementSystem : BuildingPartSystem
    {
        [SerializeField] private BuildingPlacementSettings m_settings = new BuildingPlacementSettings();

        public BuildingPlacementSettings Settings => m_settings;

        public bool DrawGizmos { get; set; }

        protected float m_pulseTimer;
        protected GameObject m_cachedDirectionIndicator;

        public override void Shutdown()
        {
            RestoreMaterials();
            ClearDirectionIndicator();

            m_cachedDirectionIndicator = null;
            m_pulseTimer = 0f;
            base.Shutdown();
        }

        public virtual void HandleStateChange(BuildingPart.BuildingState state)
        {
            bool isPlaced = state == BuildingPart.BuildingState.None || state == BuildingPart.BuildingState.Placed;
            bool isPreviewState = state == BuildingPart.BuildingState.Queue
                || state == BuildingPart.BuildingState.Placement
                || state == BuildingPart.BuildingState.Adjusting;

            Part.RendererSystem.SetCollidersEnabled(state != BuildingPart.BuildingState.Placement);
            Part.CacheSystem.SetSocketsEnabled(state != BuildingPart.BuildingState.Placement && state != BuildingPart.BuildingState.Destruction);

            if (isPreviewState)
            {
                Part.CacheSystem.SetRigidbodiesKinematic(true);
            }
            else if (isPlaced)
            {
                Part.CacheSystem.RestoreRigidbodiesKinematic();
            }

            if (isPlaced)
            {
                ClearDirectionIndicator();
                RestoreMaterials();
            }
            else if (isPreviewState || state == BuildingPart.BuildingState.Destruction)
            {
                if (isPreviewState)
                {
                    SetupDirectionIndicator();
                }
                else
                {
                    ClearDirectionIndicator();
                }
                ApplyPreviewMaterials(state);
                UpdateMaterialsColor(state);
            }

            HandleStateGameObjects(state);
        }

        public virtual void UpdatePreview(bool isValid, BuildingPart.BuildingState state)
        {
            if (state == BuildingPart.BuildingState.None || state == BuildingPart.BuildingState.Placed)
            {
                RestoreMaterials();
                return;
            }

            ApplyPreviewMaterials(state);

            BuildingPart.BuildingState colorState = (state == BuildingPart.BuildingState.Placement && !isValid)
                ? BuildingPart.BuildingState.Destruction
                : state;

            UpdateMaterialsColor(colorState);
        }

        public virtual void ApplyPreviewMaterials(BuildingPart.BuildingState state)
        {
            if (!m_settings.EnablePreviewMaterial)
            {
                return;
            }

            PreviewStateMaterialData config = GetConfigForState(state);
            if (config?.MaterialMode != MaterialMode.ReplaceMaterial)
            {
                return;
            }

            RendererVariantData variant = Part.RendererSystem?.Active;
            if (variant?.Renderers == null)
            {
                return;
            }

            int stateHash = state.GetHashCode();
            Material[][] previewMaterials = Part.CacheSystem.GetOrCreatePreviewMaterials(stateHash, config.CustomMaterial);

            if (previewMaterials == null)
            {
                return;
            }

            for (int i = 0; i < variant.Renderers.Length && i < previewMaterials.Length; i++)
            {
                if (variant.Renderers[i] && previewMaterials[i] != null)
                {
                    variant.Renderers[i].sharedMaterials = previewMaterials[i];
                }
            }
        }

        public virtual void RestoreMaterials()
        {
            if (Part?.CacheSystem == null || Part.RendererSystem?.Active == null)
            {
                return;
            }

            RendererVariantData variant = Part.RendererSystem.Active;
            if (variant.Renderers == null)
            {
                return;
            }

            Part.CacheSystem.RestoreOriginalMaterials();
        }

        public virtual void UpdateMaterialsColor(BuildingPart.BuildingState state)
        {
            PreviewStateMaterialData config = GetConfigForState(state);
            if (config == null)
            {
                return;
            }

            RendererVariantData variant = Part.RendererSystem?.Active;
            if (variant?.Renderers == null)
            {
                return;
            }

            Color targetColor = config.Color;
            Color finalColor = m_settings.PreviewMaterialTransition == MaterialTransitionType.Pulse
                ? ApplyPulse(targetColor)
                : targetColor;

            if (Part.CacheSystem == null)
            {
                return;
            }

            MaterialPropertyBlock block = Part.CacheSystem.GetMaterialPropertyBlock();

            for (int i = 0; i < variant.Renderers.Length; i++)
            {
                if (!variant.Renderers[i])
                {
                    continue;
                }

                variant.Renderers[i].GetPropertyBlock(block);
                block.SetColor(config.ColorPropertyName, finalColor);
                variant.Renderers[i].SetPropertyBlock(block);
            }
        }

        public virtual void SetupDirectionIndicator()
        {
            if (!m_settings.PreviewUseDirectionIndicator || m_settings.PreviewDirectionIndicatorPrefab == null)
            {
                return;
            }

            ClearDirectionIndicator();

            m_cachedDirectionIndicator = new GameObject("DirectionIndicator");
            m_cachedDirectionIndicator.transform.SetParent(Part.transform, false);

            GameObject indicator = UnityEngine.Object.Instantiate(
                m_settings.PreviewDirectionIndicatorPrefab,
                m_cachedDirectionIndicator.transform);

            indicator.transform.localPosition = m_settings.PreviewDirectionIndicatorPosition;
            indicator.transform.localEulerAngles = m_settings.PreviewDirectionIndicatorRotation;
            indicator.transform.localScale = m_settings.PreviewDirectionIndicatorScale;
        }

        public virtual bool HasDirectionIndicator() => m_cachedDirectionIndicator != null;

        public virtual void ClearDirectionIndicator()
        {
            if (m_cachedDirectionIndicator != null)
            {
                UnityEngine.Object.DestroyImmediate(m_cachedDirectionIndicator);
                m_cachedDirectionIndicator = null;
            }
        }

        protected virtual PreviewStateMaterialData GetConfigForState(BuildingPart.BuildingState state)
        {
            if (m_settings.PreviewStateMaterials == null)
            {
                return null;
            }

            for (int i = 0; i < m_settings.PreviewStateMaterials.Length; i++)
            {
                if (m_settings.PreviewStateMaterials[i].State == state)
                {
                    return m_settings.PreviewStateMaterials[i];
                }
            }

            return null;
        }

        protected virtual Color ApplyPulse(Color color)
        {
            m_pulseTimer += Time.deltaTime * m_settings.PulseFrequency;
            color.a = Mathf.Lerp(m_settings.PulseMinAlpha, m_settings.PulseMaxAlpha, Mathf.PingPong(m_pulseTimer, 1f));
            return color;
        }

        protected virtual void HandleStateGameObjects(BuildingPart.BuildingState state)
        {
            if (m_settings.StateGameObjects == null)
            {
                return;
            }

            for (int i = 0; i < m_settings.StateGameObjects.Length; i++)
            {
                if (m_settings.StateGameObjects[i].State == state)
                {
                    m_settings.StateGameObjects[i].Apply();
                }
            }
        }
    }
}