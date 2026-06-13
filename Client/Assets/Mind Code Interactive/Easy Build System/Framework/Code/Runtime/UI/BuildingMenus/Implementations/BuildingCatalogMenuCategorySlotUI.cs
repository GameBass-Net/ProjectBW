/// <summary>
/// Project : Easy Build System
/// Class : BuildingCatalogMenuCategorySlotUI.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;
using UnityEngine.UI;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Implementations
{
    public class BuildingCatalogMenuCategorySlotUI : MonoBehaviour
    {
        [SerializeField] private Button m_categoryButton;
        [SerializeField] private Image m_selectedBlankImage;
        [SerializeField] private Image m_selectedImage;
        [SerializeField] private Color m_unselectedColor;
        [SerializeField] private Color m_selectedColor;

        public Button Button => m_categoryButton;

        public void SetSelected(bool selected)
        {
            if (m_selectedBlankImage != null)
            {
                m_selectedBlankImage.enabled = selected;
            }

            if (m_selectedImage != null)
            {
                m_selectedImage.color = selected ? m_selectedColor : m_unselectedColor;
            }
        }
    }
}