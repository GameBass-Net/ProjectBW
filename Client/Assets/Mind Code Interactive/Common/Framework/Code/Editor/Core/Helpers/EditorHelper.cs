/// <summary>
/// Project : Mind Code Interactive
/// Class : EditorHelper.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Editor.Core.Helpers
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using UnityEngine;

namespace MindCodeInteractive.Common.Framework.Code.Editor.Core.Helpers
{
    public static class EditorHelper
    {
        public static bool IsInPrefabIsolation(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (!gameObject)
            {
                return false;
            }

            if (UnityEditor.SceneManagement.EditorSceneManager.IsPreviewSceneObject(gameObject))
            {
                return true;
            }

#if UNITY_2020_1_OR_NEWER
            UnityEditor.SceneManagement.PrefabStage prefabStage =
                UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
#else
            UnityEditor.Experimental.SceneManagement.PrefabStage prefabStage =
                UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
#endif

            return prefabStage != null && gameObject.scene == prefabStage.scene;
#else
            return false;
#endif
        }
    }
}