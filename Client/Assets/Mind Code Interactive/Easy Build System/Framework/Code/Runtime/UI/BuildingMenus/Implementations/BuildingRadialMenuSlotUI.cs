/// <summary>
/// Project : Easy Build System
/// Class : BuildingRadialMenuSlotUI.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
{
    public class BuildingRadialMenuSlotUI : BuildingSlotUI
    {
        [SerializeField] private float m_normalScale = 1f;
        [SerializeField] private float m_highlightScale = 1.25f;
        [SerializeField] private Color m_normalColor = new Color(1, 1, 1, 0.5f);
        [SerializeField] private Color m_highlightColor = Color.white;
        [SerializeField] private Color m_disabledColor = new Color(1, 1, 1, 0.3f);

        public float NormalScale { get => m_normalScale; set => m_normalScale = value; }

        public float HighlightScale { get => m_highlightScale; set => m_highlightScale = value; }

        public Color NormalColor { get => m_normalColor; set => m_normalColor = value; }

        public Color HighlightColor { get => m_highlightColor; set => m_highlightColor = value; }

        public Color DisabledColor { get => m_disabledColor; set => m_disabledColor = value; }

        public override void SetHighlight(bool highlighted)
        {
            float targetScale = highlighted ? m_highlightScale : m_normalScale;
            transform.localScale = Vector3.one * targetScale;

            if (IconImage != null && m_isEnabled)
            {
                Color targetColor = highlighted ? m_highlightColor : m_normalColor;
                IconImage.color = targetColor;
            }
        }

        public override void SetEnabled(bool enabled)
        {
            base.SetEnabled(enabled);

            if (IconImage == null)
            {
                return;
            }

            IconImage.color = enabled ? m_normalColor : m_disabledColor;
        }

        public override void OnSlotClicked() { }
    }
}