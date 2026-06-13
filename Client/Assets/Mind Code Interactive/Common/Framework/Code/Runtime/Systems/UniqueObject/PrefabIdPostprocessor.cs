/// <summary>
/// Project : Mind Code Interactive
/// Class : PrefabIdPostprocessor.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

using MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject.Abstracts;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.UniqueObject
{
    class PrefabIdPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string path in importedAssets)
            {
                GameObject prefabGameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabGameObject == null)
                {
                    continue;
                }

                BaseUniqueObject uniqueObject = prefabGameObject.GetComponent<BaseUniqueObject>();
                if (uniqueObject != null)
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);

                    if (uniqueObject.PrefabId != guid)
                    {
                        uniqueObject.SetPrefabId(guid);
                        EditorUtility.SetDirty(uniqueObject);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
    }
}
#endif