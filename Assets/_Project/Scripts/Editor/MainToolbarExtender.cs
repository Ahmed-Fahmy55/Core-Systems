using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;

public static class MainToolbarExtender
{
    private const string DropdownID = "Zone8/Scene Selector";
    private const string FpsSlider = "Zone8/FPS Slider";
    private const string TimeScaleSlider = "Zone8/Time Scale";

    [MainToolbarElement(DropdownID)]
    public static MainToolbarElement CreateSceneDropdown()
    {
        var content = new MainToolbarContent("Select Scene", null, "Switch build scenes");

        return new MainToolbarDropdown(content, (rect) =>
        {
            GenericMenu menu = new GenericMenu();
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;

            if (sceneCount == 0)
            {
                menu.AddDisabledItem(new GUIContent("No scenes in Build Settings"));
            }

            for (int i = 0; i < sceneCount; i++)
            {
                string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);

                menu.AddItem(new GUIContent(name), false, () =>
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(path);
                    }
                });
            }
            menu.DropDown(rect);
        });
    }

    [MainToolbarElement(FpsSlider)]
    public static MainToolbarElement CreateFPSSlider()
    {
        var content = new MainToolbarContent("FPS Limit", null, "Adjust Application.targetFrameRate");

        int currentLimit = Application.targetFrameRate;
        float initialValue = currentLimit <= 0 ? 60f : currentLimit;

        var slider = new MainToolbarSlider(content, initialValue, 10f, 240f, (newValue) =>
        {
            int roundedFPS = Mathf.RoundToInt(newValue);
            Application.targetFrameRate = roundedFPS;
        });

        return slider;
    }

    [MainToolbarElement(TimeScaleSlider)]
    public static MainToolbarElement CreateTimeScaleSlider()
    {
        var icon = EditorGUIUtility.IconContent("d_WaitSpin").image as Texture2D;
        var content = new MainToolbarContent("Time Scale", icon, "Adjust Game Speed (Time.timeScale)");

        float initialValue = Time.timeScale;

        var slider = new MainToolbarSlider(content, initialValue, 0f, 3f, (newValue) =>
        {
            Time.timeScale = newValue;
            if (newValue == 0) Debug.Log("Game Paused via Toolbar");
        });
        return slider;
    }
}