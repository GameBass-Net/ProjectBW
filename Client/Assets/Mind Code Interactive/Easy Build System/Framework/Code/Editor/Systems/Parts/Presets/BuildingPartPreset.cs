/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartPreset.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Presets
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;

using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Behaviors.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Conditions.Abstracts;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Implementations.Placements.Data;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets;
using MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Sockets.Data;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Editor.Systems.Parts.Presets
{
    [Serializable]
    public class BehaviorData
    {
        public string TypeName;
        public string JsonData;

        public BehaviorData(BuildingBehavior behavior)
        {
            TypeName = behavior.GetType().AssemblyQualifiedName;
            JsonData = JsonUtility.ToJson(behavior);
        }
    }

    [Serializable]
    public class ConditionData
    {
        public string TypeName;
        public string JsonData;

        public ConditionData(BuildingCondition condition)
        {
            TypeName = condition.GetType().AssemblyQualifiedName;
            JsonData = JsonUtility.ToJson(condition);
        }
    }

    [Serializable]
    public class SocketPresetData
    {
        public string ParentPath;
        public string ObjectName;
        public string SocketType;
        public int SocketProperty;
        public float SocketRadius;
        public Vector3 LocalPosition;
        public Vector3 LocalRotationEuler;
        public Vector3 LocalScale;
        public List<SocketSnapData> SnappingPoints = new List<SocketSnapData>();
    }

    public class BuildingPartPreset : ScriptableObject
    {
        [SerializeField] private string m_presetName;
        [SerializeField] private string m_category = "Default";
        [SerializeField] private GameObject m_sourcePrefab;
        [SerializeField] private BuildingPlacementSettings m_placementSettings = new BuildingPlacementSettings();
        [SerializeField] private List<BehaviorData> m_behaviorsData = new List<BehaviorData>();
        [SerializeField] private List<ConditionData> m_conditionsData = new List<ConditionData>();
        [SerializeField] private List<SocketPresetData> m_socketsData = new List<SocketPresetData>();

        public string PresetName => m_presetName;
        public string Category => m_category;
        public GameObject SourcePrefab { get => m_sourcePrefab; set => m_sourcePrefab = value; }
        public BuildingPlacementSettings PlacementSettings => m_placementSettings;
        public IReadOnlyList<SocketPresetData> SocketsData => m_socketsData;

        public void ApplyToPart(BuildingPart part, bool applySockets)
        {
            if (part == null)
            {
                return;
            }

            part.Category = m_category;
            CopySettings(m_placementSettings, part.PlacementSystem.Settings);
            ApplyBehaviors(part);
            ApplyConditions(part);

            if (applySockets && m_socketsData.Count > 0)
            {
                ApplySockets(part);
            }
        }

        public void ConfigureFromPart(BuildingPart part, string presetName = null, bool includeSockets = false)
        {
            if (part == null)
            {
                return;
            }

            m_presetName = presetName ?? part.name + "_Preset";
            m_category = part.Category;
            CopySettings(part.PlacementSystem.Settings, m_placementSettings);

            m_behaviorsData = part.BehaviorSystem.GetAllBehaviors()
                .Where(b => b != null)
                .Select(b => new BehaviorData(b))
                .ToList();

            m_conditionsData = part.ConditionSystem.GetAllConditions()
                .Where(c => c != null)
                .Select(c => new ConditionData(c))
                .ToList();

            m_socketsData.Clear();
            if (includeSockets)
            {
                CaptureSocketsFromPart(part);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public bool HasChangesFrom(BuildingPart part)
        {
            if (part == null)
            {
                return false;
            }

            if (m_category != part.Category)
            {
                return true;
            }

            if (JsonUtility.ToJson(m_placementSettings) != JsonUtility.ToJson(part.PlacementSystem.Settings))
            {
                return true;
            }

            if (!AreBehaviorsEqual(part))
            {
                return true;
            }

            if (!AreConditionsEqual(part))
            {
                return true;
            }

            return false;
        }

        #region Behaviors

        private void ApplyBehaviors(BuildingPart part)
        {
            BuildingBehaviorSystem system = part.BehaviorSystem;

            foreach (BuildingBehavior b in system.GetAllBehaviors().ToList())
            {
                if (b != null)
                {
                    system.RemoveBehavior(b.GetType());
                }
            }

            foreach (BehaviorData data in m_behaviorsData)
            {
                Type type = ResolveType(data.TypeName);
                if (type == null)
                {
                    continue;
                }

                BuildingBehavior component = system.AddBehavior(type);
                if (component == null)
                {
                    continue;
                }

                try { JsonUtility.FromJsonOverwrite(data.JsonData, component); } catch { }
                SetPartField(component, part);
            }
        }

        private bool AreBehaviorsEqual(BuildingPart part)
        {
            List<BuildingBehavior> valid = part.BehaviorSystem.GetAllBehaviors().Where(b => b != null).ToList();
            if (m_behaviorsData.Count != valid.Count)
            {
                return false;
            }

            for (int i = 0; i < valid.Count; i++)
            {
                if (valid[i].GetType().FullName != GetTypeFullName(m_behaviorsData[i].TypeName))
                {
                    return false;
                }

                if (StripInstanceIds(JsonUtility.ToJson(valid[i])) != StripInstanceIds(m_behaviorsData[i].JsonData))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Conditions

        private void ApplyConditions(BuildingPart part)
        {
            BuildingConditionSystem system = part.ConditionSystem;

            foreach (BuildingCondition c in system.GetAllConditions().ToList())
            {
                if (c != null)
                {
                    system.RemoveCondition(c.GetType());
                }
            }

            foreach (ConditionData data in m_conditionsData)
            {
                Type type = ResolveType(data.TypeName);
                if (type == null)
                {
                    continue;
                }

                BuildingCondition component = system.AddCondition(type);
                if (component == null)
                {
                    continue;
                }

                try { JsonUtility.FromJsonOverwrite(data.JsonData, component); } catch { }
                SetPartField(component, part);
            }
        }

        private bool AreConditionsEqual(BuildingPart part)
        {
            List<BuildingCondition> valid = part.ConditionSystem.GetAllConditions().Where(c => c != null).ToList();
            if (m_conditionsData.Count != valid.Count)
            {
                return false;
            }

            for (int i = 0; i < valid.Count; i++)
            {
                if (valid[i].GetType().FullName != GetTypeFullName(m_conditionsData[i].TypeName))
                {
                    return false;
                }

                if (StripInstanceIds(JsonUtility.ToJson(valid[i])) != StripInstanceIds(m_conditionsData[i].JsonData))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Settings

        private void CopySettings(BuildingPlacementSettings source, BuildingPlacementSettings target)
        {
            if (source == null || target == null)
            {
                return;
            }

            foreach (FieldInfo field in typeof(BuildingPlacementSettings).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                bool isSerializable = field.GetCustomAttributes(typeof(SerializeField), true).Length > 0 ||
                                      (field.IsPublic && field.GetCustomAttributes(typeof(NonSerializedAttribute), true).Length == 0);

                if (isSerializable)
                {
                    try { field.SetValue(target, field.GetValue(source)); } catch { }
                }
            }
        }

        #endregion

        #region Sockets

        private void CaptureSocketsFromPart(BuildingPart part)
        {
            foreach (BuildingSocket socket in part.CacheSystem.Sockets)
            {
                m_socketsData.Add(new SocketPresetData
                {
                    ParentPath = GetRelativePath(socket.transform.parent, part.transform),
                    ObjectName = socket.transform.name,
                    SocketType = socket.SocketType,
                    SocketProperty = socket.SocketProperty,
                    SocketRadius = socket.SocketRadius,
                    LocalPosition = socket.transform.localPosition,
                    LocalRotationEuler = socket.transform.localRotation.eulerAngles,
                    LocalScale = socket.transform.localScale,
                    SnappingPoints = socket.SnappingPoints?.Select(CloneSnapData).ToList() ?? new List<SocketSnapData>()
                });
            }
        }

        private void ApplySockets(BuildingPart part)
        {
            foreach (BuildingSocket socket in part.CacheSystem.Sockets)
            {
#if UNITY_EDITOR
                UnityEditor.Undo.DestroyObjectImmediate(socket.gameObject);
#else
                Destroy(socket.gameObject);
#endif
            }

            foreach (SocketPresetData data in m_socketsData)
            {
                Transform parent = GetOrCreatePath(part.transform, data.ParentPath);
                GameObject socketObj = new GameObject(data.ObjectName);
#if UNITY_EDITOR
                UnityEditor.Undo.RegisterCreatedObjectUndo(socketObj, "Create Socket");
#endif
                socketObj.transform.SetParent(parent, false);
                socketObj.transform.localPosition = data.LocalPosition;
                socketObj.transform.localRotation = Quaternion.Euler(data.LocalRotationEuler);
                socketObj.transform.localScale = data.LocalScale;

                BuildingSocket socket = socketObj.AddComponent<BuildingSocket>();
                socket.SocketType = data.SocketType;
                socket.SocketProperty = data.SocketProperty;
                socket.SocketRadius = data.SocketRadius;
                socket.SnappingPoints = data.SnappingPoints?.Select(CloneSnapData).ToList() ?? new List<SocketSnapData>();
            }
        }

        private static SocketSnapData CloneSnapData(SocketSnapData source)
        {
            if (source == null)
            {
                return null;
            }

            SocketSnapData clone = new SocketSnapData();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), clone);
            return clone;
        }

        private static Transform GetOrCreatePath(Transform root, string path)
        {
            if (root == null || string.IsNullOrEmpty(path))
            {
                return root;
            }

            Transform current = root;
            foreach (string part in path.Split('/').Where(p => !string.IsNullOrEmpty(p)))
            {
                Transform child = current.Find(part);
                if (!child)
                {
                    child = new GameObject(part).transform;
                    child.SetParent(current, false);
                }
                current = child;
            }
            return current;
        }

        private static string GetRelativePath(Transform target, Transform root)
        {
            if (target == null || root == null)
            {
                return string.Empty;
            }

            Stack<string> pathStack = new Stack<string>();
            for (Transform i = target; i != null && i != root; i = i.parent)
            {
                pathStack.Push(i.name);
            }

            return string.Join("/", pathStack);
        }

        #endregion

        #region Helpers

        private void SetPartField(UnityEngine.Object component, BuildingPart part)
        {
            FieldInfo partField = component.GetType().BaseType?.GetField("m_part", BindingFlags.NonPublic | BindingFlags.Instance);
            partField?.SetValue(component, part);
        }

        private static Type ResolveType(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        private string GetTypeFullName(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                return string.Empty;
            }

            int commaIndex = assemblyQualifiedName.IndexOf(',');
            return commaIndex > 0 ? assemblyQualifiedName.Substring(0, commaIndex) : assemblyQualifiedName;
        }

        private string StripInstanceIds(string json)
        {
            return Regex.Replace(json, @"""instanceID"":\s*-?\d+", @"""instanceID"":0");
        }

        #endregion
    }
}