#if UNITY_EDITOR
using Zone8.Screens;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScreenManager))]
public class ScreenManagerEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Screen controls are only available in Play mode.", MessageType.Info);
            return;
        }

        var manager = (ScreenManager)target;

        if (manager == null)
            return;

        // Use reflection to access the private _screenInstances dictionary
        var dictField = typeof(ScreenManager).GetField("_screenInstances", BindingFlags.NonPublic | BindingFlags.Instance);
        var dict = dictField?.GetValue(manager) as IDictionary;

        if (dict == null)
        {
            EditorGUILayout.HelpBox("_screenInstances dictionary is null or not initialized.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Screen Controls", EditorStyles.boldLabel);


        foreach (DictionaryEntry entry in dict)
        {
            var eScreen = entry.Key as EScreen;
            var screen = entry.Value as Zone8.Screens.ScreenBase;

            EditorGUILayout.BeginHorizontal();

            string label = "Null";
            if (entry.Key != null)
            {
                var screenObj = entry.Key;
                var screenNameProp = screenObj.GetType().GetProperty("ScreenName", BindingFlags.Public | BindingFlags.Instance);
                if (screenNameProp != null)
                {
                    label = screenNameProp.GetValue(screenObj) as string ?? screenObj.ToString();
                }
                else
                {
                    label = screenObj.ToString();
                }
            }
            EditorGUILayout.LabelField(label, GUILayout.Width(200));

            GUI.enabled = eScreen != null && screen != null;
            if (GUILayout.Button("Show", GUILayout.Width(60)))
            {
                manager.ShowScreenSO(eScreen);
            }
            if (GUILayout.Button("Hide", GUILayout.Width(60)))
            {
                manager.HideScreenSO(eScreen);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }
    }

}
#endif