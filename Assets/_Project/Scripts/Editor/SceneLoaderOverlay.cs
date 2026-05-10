using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;

public static class Zone8MainToolbarExtender
{
    // Unity uses this string for the right-click menu label.
    private const string DropdownID = "Zone8/Scene Selector";
    private const string RefreshID = "Zone8/Refresh Scenes";

    [MainToolbarElement(DropdownID)]
    public static MainToolbarElement CreateSceneDropdown()
    {
        // The first argument here is what shows UPON the button in the toolbar
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

    [MainToolbarElement(RefreshID)]
    public static MainToolbarElement CreateRefreshButton()
    {
        var icon = EditorGUIUtility.IconContent("Refresh").image as Texture2D;
        var content = new MainToolbarContent(null, icon, "Refresh Scene Toolbar");

        return new MainToolbarButton(content, () =>
        {
            MainToolbar.Refresh(DropdownID);
            Debug.Log("Zone8 Scene List Updated");
        });
    }
}