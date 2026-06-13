/// <summary>
/// Project : Easy Build System
/// Class : BuildingCollection.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;

using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Core.Attributes;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Attributes;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Collections
{
    public sealed class BuildingCollection : ScriptableObject
    {
        [SerializeField, NotNull] private string m_name = "New Building Collection";
        [SerializeField, BuildingPartReference] private string[] m_partReferences = Array.Empty<string>();

        public string Name { get => m_name; set => m_name = value; }
        public string[] PartReferences => m_partReferences;
    }
}