/// <summary>
/// Project : Easy Build System
/// Class : BuildingSlotUI.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.UI.BuildingMenus.Abstracts
{
    public abstract class BuildingSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField, NotNull] private RawImage m_iconImage;
        [SerializeField] private Text m_nameText;
        [SerializeField] private Text m_countText;
        [SerializeField] private Button m_button;

        protected BuildingSlotData m_data;
        protected int m_index;
        protected Action<int> m_onClickCallback;
        protected bool m_isEnabled = true;

        private BuildingMenuUI m_menu;

        public RawImage IconImage => m_iconImage;

        public Text NameText => m_nameText;

        public Text CountText => m_countText;

        public Button Button => m_button;

        public virtual void Initialize(BuildingSlotData data, int index, Action<int> onClickCallback)
        {
            m_data = data;
            m_index = index;
            m_onClickCallback = onClickCallback;

            if (m_menu == null)
            {
                m_menu = GetComponentInParent<BuildingMenuUI>();
            }

            if (m_button != null)
            {
                m_button.onClick.RemoveAllListeners();
                m_button.onClick.AddListener(OnSlotClicked);
            }

            Refresh();
        }

        public virtual void SetData(BuildingSlotData data)
        {
            m_data = data;
            if (m_menu == null)
            {
                m_menu = GetComponentInParent<BuildingMenuUI>();
            }

            Refresh();
        }

        public virtual void SetHighlight(bool highlighted) { }

        public virtual void SetEnabled(bool enabled)
        {
            m_isEnabled = enabled;
            if (m_button != null)
            {
                m_button.interactable = enabled;
            }

            SetAlpha(m_iconImage, enabled ? 1f : 0.5f);
            SetAlpha(m_nameText, enabled ? 1f : 0.5f);
        }

        public virtual void Refresh()
        {
            if (m_data == null)
            {
                return;
            }

            if (m_menu == null)
            {
                m_menu = GetComponentInParent<BuildingMenuUI>();
            }

            m_data.EnsureAction(m_menu);

            Texture2D slotIcon = m_data.GetIcon();
            if (m_iconImage != null)
            {
                m_iconImage.texture = slotIcon;
                m_iconImage.enabled = slotIcon != null;
            }

            if (m_nameText != null)
            {
                m_nameText.text = m_data.Name;
            }

            int? remainingCount = m_data.RemainingCount;
            if (m_countText != null)
            {
                bool shouldShowCount = remainingCount.HasValue && remainingCount.Value > 0;
                m_countText.enabled = shouldShowCount;
                if (shouldShowCount)
                {
                    m_countText.text = remainingCount.Value > 1 ? remainingCount.Value.ToString() : "";
                }
            }

            SetEnabled(m_data.IsAvailable);
        }

        public virtual void OnSlotClicked() => m_onClickCallback?.Invoke(m_index);

        public virtual void OnPointerClick(PointerEventData eventData) => OnSlotClicked();

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color graphicColor = graphic.color;
            graphicColor.a = alpha;
            graphic.color = graphicColor;
        }
    }
}