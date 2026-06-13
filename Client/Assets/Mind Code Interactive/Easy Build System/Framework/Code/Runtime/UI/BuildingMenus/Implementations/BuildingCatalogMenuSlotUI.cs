/// <summary>
/// Project : Easy Build System
/// Class : BuildingCatalogMenuSlotUI.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;
using UnityEngine.UI;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
{
    public class BuildingCatalogMenuSlotUI : BuildingSlotUI
    {
        [SerializeField] private Image m_highlight;
        [SerializeField] private Color m_normalColor = Color.white;
        [SerializeField] private Color m_disabledColor = new Color(1, 1, 1, 0.3f);

        public override void SetHighlight(bool highlighted)
        {
            if (m_highlight != null)
            {
                m_highlight.enabled = highlighted;
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
    }
}