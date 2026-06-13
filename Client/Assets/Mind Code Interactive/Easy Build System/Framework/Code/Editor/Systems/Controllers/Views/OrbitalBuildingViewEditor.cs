/// <summary>
/// Project : Easy Build System
/// Class : OrbitalBuildingViewEditor.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Views
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Editor.Tools.Inspectors.Styles;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Controllers.Views.Implementations;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Controllers.Views
{
    [CustomEditor(typeof(OrbitalBuildingView), true)]
    public class OrbitalBuildingViewEditor : BuildingViewEditor
    {
        protected override void OnInspectorDraw()
        {
            base.OnInspectorDraw();

            EditorGUIExtended.Separator("Orbital Settings", true);
            Properties.Draw("m_originTransform", new GUIContent("Origin Transform", "Transform around which the camera orbits for orbital view."));
        }
    }
}