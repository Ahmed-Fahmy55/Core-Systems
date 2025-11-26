using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadingWindow : OdinEditorWindow
{
    [MenuItem("Zone8/Scene Loader")]
    private static void OpenWindow()
    {
        GetWindow<SceneLoadingWindow>().Show();
    }

    [ShowInInspector, TableList]
    private SceneInfo[] _scenes;

    private void OnValidate()
    {
        LoadScenesFromBuildSettings();
    }

    private void LoadScenesFromBuildSettings()
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        _scenes = new SceneInfo[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            _scenes[i] = new SceneInfo { SceneName = sceneName, ScenePath = scenePath };
        }
    }

    [Button(ButtonSizes.Large)]
    public void RefreshSceneList()
    {
        LoadScenesFromBuildSettings();
    }

    [System.Serializable]
    public class SceneInfo
    {
        [TableColumnWidth(200, Resizable = false)]
        [ReadOnly]
        public string SceneName;

        [HideInInspector]
        public string ScenePath;

        [Button("Open Scene")]
        public void OpenInEditor()
        {
            if (!string.IsNullOrEmpty(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
            }
            else
            {
                Debug.LogWarning($"Scene '{SceneName}' path is empty. Please check the build settings.");
            }
        }
    }
}
