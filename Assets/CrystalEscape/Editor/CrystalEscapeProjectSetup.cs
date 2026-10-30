#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalEscape.Editor
{
    [InitializeOnLoad]
    public static class CrystalEscapeProjectSetup
    {
        private const string ScenePath = "Assets/CrystalEscape/Scenes/MainGame.unity";

        static CrystalEscapeProjectSetup()
        {
            EditorApplication.delayCall += PrepareProject;
        }

        [MenuItem("Crystal Escape/Prepare Project")]
        public static void PrepareProject()
        {
            if (Application.isPlaying) return;

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (sceneAsset == null)
            {
                Directory.CreateDirectory("Assets/CrystalEscape/Scenes");
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.Refresh();
            }

            bool alreadyIncluded = false;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == ScenePath)
                {
                    alreadyIncluded = true;
                    break;
                }
            }

            if (!alreadyIncluded)
            {
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            }

            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Debug.Log("Crystal Escape is ready. Press Play to test the complete placeholder game.");
        }

        [MenuItem("Crystal Escape/Build Windows")]
        public static void BuildWindows()
        {
            PrepareProject();
            Directory.CreateDirectory("Builds/Windows");
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/CrystalEscape.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildPipeline.BuildPlayer(options);
        }

        [MenuItem("Crystal Escape/Build WebGL")]
        public static void BuildWebGL()
        {
            PrepareProject();
            Directory.CreateDirectory("Builds/WebGL");
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            BuildPipeline.BuildPlayer(options);
        }
    }
}
#endif

