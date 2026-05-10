using UnityEditor;
using UnityEngine;

namespace ASoliman.Utils.ClipboardPlus
{
    /// <summary>
    /// Provides editor shortcuts for clipboard operations in the Unity Editor.
    /// </summary>
    public static class ClipboardShortcuts
    {
        [MenuItem("Tools/Clipboard Plus/Copy Selected GameObject Components &2")]
        public static void CopySelectedComponent()
        {
            /// <summary>
            /// Copies all components from the currently selected GameObject to the clipboard.
            /// </summary>
            if (Selection.activeGameObject != null)
            {
                // Copy all components on the GameObject
                foreach (var component in Selection.activeGameObject.GetComponents<Component>())
                {
                    if(SkipUnnecessaryComponents(component))
                        continue;
                    ClipboardManager.Instance.AddClip(component, ClipType.Component);
                }
                ClipboardEditorWindow.ForceRepaint();
            }
            ClipboardEditorWindow.ShowWindow();
        }

        private static bool SkipUnnecessaryComponents(Component component)
        {
            // Skip unnecessary components
            if (component is ParticleSystemRenderer)
                return true;
            return false;
        }
    }
}