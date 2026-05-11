using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;

public static class MainToolbarExtender
{
    private const string DropdownID = "Zone8/Scene Selector";
    private const string SliderID = "Zone8/FPS Slider";

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

    [MainToolbarElement(SliderID)]
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
}