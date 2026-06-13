/// <summary>
/// Project : Easy Build System
/// Class : ThirdPersonBuildingViewEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Views
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Views
{
    [CustomEditor(typeof(ThirdPersonBuildingView), true)]
    public class ThirdPersonBuildingViewEditor : BuildingViewEditor
    {
        protected override void OnInspectorDraw()
        {
            base.OnInspectorDraw();

            EditorGUIExtended.Separator("Third Person Settings", true);
            Properties.Draw("m_originTransform", new GUIContent("Origin Transform", "Transform used as the pivot point for third person view."));
        }
    }
}