using System.IO;
using Digger.Modules.Core.Sources;
using Digger.Modules.Core.Sources.Jobs;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Digger.Modules.Runtime.Sources.Editor
{
    public class PreprocessBuild : BuildPlayerProcessor
    {
        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            if (Application.isPlaying)
                return;

            Debug.Log("PreprocessBuild.PrepareForBuild");
            NativeCollectionsPool.Instance.Dispose();

            var streamingAssetsBasePath = Path.Combine(Application.streamingAssetsPath, "DiggerData");
            if (Directory.Exists(streamingAssetsBasePath))
                Directory.Delete(streamingAssetsBasePath, true);

            var scenes = buildPlayerContext.BuildPlayerOptions.scenes;
            foreach (var scenePath in scenes) {
                var scene = SceneManager.GetSceneByPath(scenePath);
                var sceneLoaded = scene.IsValid() && scene.isLoaded;
                if (!sceneLoaded) {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }

                ProcessScene(scene);

                if (!sceneLoaded) {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private void ProcessScene(Scene scene)
        {
            Debug.Log($"PreprocessBuild.ProcessScene: {scene.name}");
            var rootObjects = scene.GetRootGameObjects();

            var includeVoxelData = false;
            foreach (var rootObject in rootObjects) {
                var diggerRuntime = rootObject.GetComponentInChildren<DiggerMasterRuntime>();
                if (diggerRuntime) {
                    includeVoxelData = true;
                    Debug.Log($"DiggerMasterRuntime has been detected in scene '{scene.name}'. Voxel data will be included in build.");
                    break;
                }
            }

            foreach (var rootObject in rootObjects) {
                var diggers = rootObject.GetComponentsInChildren<DiggerSystem>();
                foreach (var digger in diggers) {
                    digger.OnPreprocessBuild(includeVoxelData);
                }
            }
        }
    }
}