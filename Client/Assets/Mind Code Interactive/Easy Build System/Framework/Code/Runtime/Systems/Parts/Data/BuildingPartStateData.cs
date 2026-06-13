/// <summary>
/// Project : Easy Build System
/// Class : BuildingPartStateData.cs
/// Namespace : MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;

using UnityEngine;

namespace MindCodeInteractive.EasyBuildSystem.Framework.Code.Runtime.Systems.Parts.Data
{
    [Serializable]
    public class BuildingPartStateData
    {
        [Serializable]
        private struct Vector3Wrapper
        {
            public Vector3 Value;
        }

        [SerializeField] private List<string> m_keys = new List<string>();
        [SerializeField] private List<string> m_values = new List<string>();

        public void SetInt(string key, int value)
        {
            Set(key, value.ToString());
        }

        public void SetFloat(string key, float value)
        {
            Set(key, value.ToString());
        }

        public void SetString(string key, string value)
        {
            Set(key, value);
        }

        public void SetBool(string key, bool value)
        {
            Set(key, value.ToString());
        }

        public void SetVector3(string key, Vector3 value)
        {
            Set(key, JsonUtility.ToJson(new Vector3Wrapper { Value = value }));
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            string val = Get(key);
            return string.IsNullOrEmpty(val)
                ? defaultValue
                : int.TryParse(val, out int result) ? result : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            string val = Get(key);
            return string.IsNullOrEmpty(val)
                ? defaultValue
                : float.TryParse(val, out float result) ? result : defaultValue;
        }

        public string GetString(string key, string defaultValue = "")
        {
            string val = Get(key);
            return string.IsNullOrEmpty(val) ? defaultValue : val;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            string val = Get(key);
            return string.IsNullOrEmpty(val)
                ? defaultValue
                : bool.TryParse(val, out bool result) ? result : defaultValue;
        }

        public Vector3 GetVector3(string key, Vector3 defaultValue)
        {
            string val = Get(key);
            if (string.IsNullOrEmpty(val))
            {
                return defaultValue;
            }

            try
            {
                Vector3Wrapper wrapper = JsonUtility.FromJson<Vector3Wrapper>(val);
                return wrapper.Value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool HasKey(string key)
        {
            return m_keys.Contains(key);
        }

        public void Remove(string key)
        {
            int index = m_keys.IndexOf(key);
            if (index < 0)
            {
                return;
            }

            m_keys.RemoveAt(index);
            m_values.RemoveAt(index);
        }

        public void Clear()
        {
            m_keys.Clear();
            m_values.Clear();
        }

        private void Set(string key, string value)
        {
            int index = m_keys.IndexOf(key);
            if (index >= 0)
            {
                m_values[index] = value;
                return;
            }

            m_keys.Add(key);
            m_values.Add(value);
        }

        private string Get(string key)
        {
            int index = m_keys.IndexOf(key);
            return index >= 0 ? m_values[index] : null;
        }
    }
}