#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable
{
#if UNITY_EDITOR
    public class ScriptableVariableRefDrawer<T> : OdinValueDrawer<ScriptableVariableRef<T>>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Retrieve child properties
            var useConstantProperty = this.Property.FindChild(x => x.Name == "UseConstant", false);
            var svProperty = this.Property.FindChild(x => x.Name == "Sv", false);
            var constValueProperty = this.Property.FindChild(x => x.Name == "ConstValue", false);

            // Begin horizontal group to place the dropdown next to the label
            EditorGUILayout.BeginHorizontal();

            // Draw the property label
            if (label != null)
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - 60));
            }

            // Draw the dropdown for UseConstant
            if (useConstantProperty != null)
            {
                var useConstantValue = (bool)useConstantProperty.ValueEntry.WeakSmartValue;
                var options = new GUIContent[] { new GUIContent("Use Scriptable Variable"), new GUIContent("Use Constant Value") };
                int selectedIndex = useConstantValue ? 1 : 0;
                int newIndex = EditorGUILayout.Popup(selectedIndex, options, GUILayout.Width(20));
                if (newIndex != selectedIndex)
                {
                    useConstantProperty.ValueEntry.WeakSmartValue = newIndex == 1;
                }
            }

            if (useConstantProperty != null)
            {
                var useConstant = (bool)useConstantProperty.ValueEntry.WeakSmartValue;
                if (useConstant && constValueProperty != null)
                {
                    constValueProperty.Draw();
                }
                else if (!useConstant && svProperty != null)
                {
                    svProperty.Draw();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
