using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic; // Added for Dictionary

namespace ASoliman.Utils.ClipboardPlus
{
    /// <summary>
    /// Handles the rendering and preview functionality for clipboard data in the Unity Editor.
    /// Provides visual representation of component data with expandable UI elements and property inspection.
    /// Now correctly handles scene references for preview.
    /// </summary>
    public static class ClipPreviewRenderer
    {
        // Cache for temporary objects to avoid recreating them constantly during repaint
        private static Dictionary<string, SerializedObject> _previewObjectCache = new Dictionary<string, SerializedObject>();
        private static Dictionary<string, GameObject> _tempGameObjectCache = new Dictionary<string, GameObject>();

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            ClearPreviewCache();
        }

        public static void ClearPreviewCache()
        {
            foreach (var go in _tempGameObjectCache.Values)
            {
                if (go != null) GameObject.DestroyImmediate(go);
            }
            _previewObjectCache.Clear();
            _tempGameObjectCache.Clear();
        }


        /// <summary>
        /// Calculates the total height needed to display the clip preview, considering properties.
        /// </summary>
        public static float GetPreviewHeight(ClipData clip)
        {
            if (!clip.isExpanded) return 24f; // Header height only

            float totalHeight = 24f; // Header height
            if (clip.clipType == ClipType.Component && clip.DataType != null)
            {
                try
                {
                    var serializedObject = GetOrCreateTemporarySerializedObject(clip);
                    if (serializedObject != null)
                    {
                        totalHeight += CalculatePropertyHeight(serializedObject);
                    }
                    else
                    {
                        totalHeight += 20f; // Fallback height if object creation fails
                    }
                }
                catch (Exception)
                {
                    totalHeight += 50f; // Error message height
                }
            }
             else
            {
                 totalHeight += 20f; // Default content height if not a component or type is missing
            }


            return totalHeight + 30f; // Add padding
        }

        /// <summary>
        /// Draws the complete preview UI for the given clip data.
        /// </summary>
        public static void DrawPreview(ClipData clip, Rect previewRect)
        {
            if (clip == null) return;

            DrawHeader(clip, previewRect);

            if (!clip.isExpanded) return;

            // Define content area below header
             var contentRect = new Rect(
                previewRect.x + 5,
                previewRect.y + 29, // Header height (24) + padding (5)
                previewRect.width - 10,
                previewRect.height - 34 // Adjust for header and bottom padding
            );

            // Draw background box for content area
            GUI.Box(contentRect, "", ClipboardThemeManager.BoxStyle);

            // Define inner rect considering box padding
            var innerRect = new Rect(
                contentRect.x + ClipboardThemeManager.BoxStyle.padding.left,
                contentRect.y + ClipboardThemeManager.BoxStyle.padding.top,
                contentRect.width - ClipboardThemeManager.BoxStyle.padding.horizontal,
                contentRect.height - ClipboardThemeManager.BoxStyle.padding.vertical
            );


            DrawContent(clip, innerRect); // Pass the inner rect for content drawing
        }

        private static float CalculatePropertyHeight(SerializedObject serializedObject)
        {
            float height = 0f;
            var property = serializedObject.GetIterator();
            bool enterChildren = true;

            if (property.NextVisible(enterChildren))
            {
                do
                {
                    height += EditorGUI.GetPropertyHeight(property, true) + EditorGUIUtility.standardVerticalSpacing;
                }
                while (property.NextVisible(false));
            }

            return height;
        }


        private static void DrawHeader(ClipData clip, Rect previewRect)
        {
            var headerRect = new Rect(previewRect.x, previewRect.y, previewRect.width, 24);
            EditorGUI.DrawRect(headerRect, ClipboardThemeManager.HeaderColor);

            float infoWidth = headerRect.width - 10; // Padding
            var sourceRect = new Rect(headerRect.x + 5, headerRect.y + 4, infoWidth / 2, EditorGUIUtility.singleLineHeight);
            var createdRect = new Rect(sourceRect.xMax, headerRect.y + 4, infoWidth / 2, EditorGUIUtility.singleLineHeight);

            string sourceText = FormatSourceText(clip);

            EditorGUI.LabelField(sourceRect, sourceText, ClipboardThemeManager.HeaderLabelStyle);
            EditorGUI.LabelField(createdRect, $"{clip.creationDate:MM/dd/yy - hh:mm:ss tt}", ClipboardThemeManager.HeaderLabelStyle);
        }

        private static string FormatSourceText(ClipData clip)
        {
            if (clip.clipType != ClipType.Component || clip.DataType == null)
                return string.IsNullOrEmpty(clip.sourcePath) ? (clip.sourceType ?? "Unknown Type") : clip.sourcePath;

            // Use DataType.Name for more reliable type naming
            var componentName = clip.DataType.Name;
            return $"{clip.sourcePath}.{componentName}";
        }


        private static void DrawContent(ClipData clip, Rect contentRect)
        {
            try
            {
                if (clip.clipType == ClipType.Component && clip.DataType != null)
                {
                    DrawComponentInspectorPreview(clip, contentRect);
                }
                 else if (clip.DataType == null)
                {
                    DrawErrorPreview(contentRect, "Cannot determine component type for preview.");
                }
            }
            catch (Exception e)
            {
                // Log the full exception for debugging
                Debug.LogError($"Error drawing clip preview for {clip.id}: {e}");
                DrawErrorPreview(contentRect, $"Preview Error: {e.Message}");
            }
        }

        /// <summary>
        /// Gets from cache or creates a temporary SerializedObject representing the clip's data.
        /// </summary>
        private static SerializedObject GetOrCreateTemporarySerializedObject(ClipData clip)
        {
             if (clip == null || clip.DataType == null || string.IsNullOrEmpty(clip.jsonData))
                return null;

            // Use clip.id as the cache key
             string cacheKey = clip.id;

            if (_previewObjectCache.TryGetValue(cacheKey, out var cachedSO) && 
                cachedSO != null && 
                cachedSO.targetObject != null && 
                !EditorUtility.IsPersistent(cachedSO.targetObject))
            {
                // Also verify all references are still valid
                bool referencesValid = true;
                if (clip.referenceMap != null)
                {
                    foreach (var kvp in clip.referenceMap)
                    {
                        if (kvp.Value == null)
                        {
                            referencesValid = false;
                            break;
                        }
                    }
                }
                
                if (referencesValid)
                    return cachedSO;
            }

            // Not in cache or invalid, create new one
            GameObject tempGO = null;
            SerializedObject serializedObject = null;

            try
            {
                // Determine if it's a Transform or RectTransform
                bool isTransform = clip.DataType == typeof(Transform);
                bool isRectTransform = clip.DataType == typeof(RectTransform);

                if (isRectTransform)
                {
                    // RectTransform needs a Canvas parent to behave correctly in previews sometimes
                    tempGO = new GameObject($"TempPreview_{clip.id}_Canvas", typeof(Canvas)) { hideFlags = HideFlags.HideAndDontSave };
                    var childGO = new GameObject($"TempPreview_{clip.id}", typeof(RectTransform)) { hideFlags = HideFlags.HideAndDontSave };
                    childGO.transform.SetParent(tempGO.transform);
                    var component = childGO.GetComponent<RectTransform>();
                    serializedObject = ApplyDataToTemporaryObject(clip, component);
                }
                else if (isTransform)
                {
                    tempGO = new GameObject($"TempPreview_{clip.id}") { hideFlags = HideFlags.HideAndDontSave };
                    var component = tempGO.transform; // Get existing transform
                    serializedObject = ApplyDataToTemporaryObject(clip, component);
                }
                else if (typeof(Component).IsAssignableFrom(clip.DataType))
                {
                    tempGO = new GameObject($"TempPreview_{clip.id}") { hideFlags = HideFlags.HideAndDontSave };
                    // AddComponent requires the type, which we have in clip.DataType
                    Component component = tempGO.AddComponent(clip.DataType);
                    if (component != null)
                    {
                        serializedObject = ApplyDataToTemporaryObject(clip, component);
                    }
                    else
                    {
                        Debug.LogError($"Failed to add component of type {clip.DataType.Name} for preview.");
                        if (tempGO != null) GameObject.DestroyImmediate(tempGO); // Clean up if component add failed
                        return null;
                    }
                }
                else if (clip.DataType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    // ScriptableObjects don't need a GameObject
                    ScriptableObject obj = ScriptableObject.CreateInstance(clip.DataType);
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    if (obj != null)
                    {
                        serializedObject = ApplyDataToTemporaryObject(clip, obj); // Use targetObject directly
                    }
                    else
                    {
                        Debug.LogError($"Failed to create ScriptableObject instance of type {clip.DataType.Name} for preview.");
                        return null;
                    }
                }
                else
                {
                    Debug.LogWarning($"Preview not supported for type: {clip.DataType.Name}");
                    return null;
                }


                // If successful, cache it
                if (serializedObject != null)
                {
                    _previewObjectCache[cacheKey] = serializedObject;
                    // Cache the top-level GameObject if one was created
                    if (tempGO != null)
                    {
                    _tempGameObjectCache[cacheKey] = tempGO;
                    }
                    return serializedObject;
                }
                else
                {
                    // Clean up if creation failed at the ApplyData step
                    if (tempGO != null) GameObject.DestroyImmediate(tempGO);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating temporary object for preview ({clip.DataType?.Name}): {e}");
                // Ensure cleanup on exception
                if (tempGO != null) GameObject.DestroyImmediate(tempGO);
                // Remove potentially broken cache entry
                _previewObjectCache.Remove(cacheKey);
                _tempGameObjectCache.Remove(cacheKey);
                return null;
            }
        }


        /// <summary>
        /// Applies JSON data and reference map to a temporary target object.
        /// </summary>
        private static SerializedObject ApplyDataToTemporaryObject(ClipData clip, UnityEngine.Object targetObject)
        {
             if (targetObject == null || string.IsNullOrEmpty(clip.jsonData))
                 return null;

             try
             {
                // Apply JSON data first
                EditorJsonUtility.FromJsonOverwrite(clip.jsonData, targetObject);

                // Apply references from the map
                if (clip.referenceMap != null && clip.referenceMap.Count > 0)
                {
                    SerializedObject serializedTarget = new SerializedObject(targetObject);
                    bool appliedAnyReference = false;

                    foreach (var kvp in clip.referenceMap)
                    {
                        string propertyPath = kvp.Key;
                        UnityEngine.Object referenceValue = kvp.Value; // This should be the actual object loaded by ClipboardManager

                        if (referenceValue != null) // Only apply if the reference could be restored
                        {
                            SerializedProperty property = serializedTarget.FindProperty(propertyPath);
                            if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                // Check if the type matches (or is assignable) to avoid errors
                                if (property.objectReferenceValue == null ||
                                    property.objectReferenceValue.GetType() == referenceValue.GetType() ||
                                     property.objectReferenceValue.GetType().IsAssignableFrom(referenceValue.GetType())) // More robust check
                                {
                                    property.objectReferenceValue = referenceValue;
                                     appliedAnyReference = true;
                                    // Debug.Log($"Applied preview reference at path: {propertyPath}, object: {referenceValue.name}");
                                }
                                else
                                {
                                    // This can happen if the field type changed or the reference is incompatible
                                    Debug.LogWarning($"Preview: Type mismatch for reference at path: {propertyPath}. Expected assignable from {property.objectReferenceValue?.GetType().Name}, got {referenceValue.GetType().Name}. Skipping.");
                                }
                            }
                            // else { Debug.LogWarning($"Preview: Could not find property at path: {propertyPath} or not an ObjectReference."); } // Less verbose logging
                        }
                        // else { Debug.LogWarning($"Preview: Reference at path {propertyPath} was null, skipping."); }
                    }

                     if (appliedAnyReference)
                     {
                        serializedTarget.ApplyModifiedPropertiesWithoutUndo(); // Apply changes for preview
                     }
                    // We return the SerializedObject we created, no need to dispose yet
                    return serializedTarget;
                }
                else
                {
                    // No references, just return a new SerializedObject based on the JSON applied state
                    return new SerializedObject(targetObject);
                }
             }
             catch (Exception e)
             {
                 Debug.LogError($"Error applying data to temporary object ({targetObject.GetType().Name}): {e}");
                return null;
             }
        }

        /// <summary>
        /// Draws the component preview using a temporary SerializedObject.
        /// </summary>
        private static void DrawComponentInspectorPreview(ClipData clip, Rect rect) // Takes innerRect
        {
            var serializedObject = GetOrCreateTemporarySerializedObject(clip);

            if (serializedObject != null && serializedObject.targetObject != null)
            {
                // Force update references before drawing
                serializedObject = RefreshObjectReferences(clip, serializedObject);
                DrawSerializedProperties(serializedObject, rect);
            }
            else if (clip.DataType != null)
            {
                DrawErrorPreview(rect, $"Could not create preview for {clip.DataType.Name}.");
            }
        }

        private static SerializedObject RefreshObjectReferences(ClipData clip, SerializedObject serializedObject)
        {
            // Re-apply references to ensure they're up to date
            if (clip.referenceMap != null && clip.referenceMap.Count > 0)
            {
                foreach (var kvp in clip.referenceMap)
                {
                    string propertyPath = kvp.Key;
                    UnityEngine.Object referenceValue = kvp.Value;
                    
                    if (referenceValue != null)
                    {
                        SerializedProperty property = serializedObject.FindProperty(propertyPath);
                        if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            property.objectReferenceValue = referenceValue;
                        }
                    }
                }
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            return serializedObject;
        }


        /// <summary>
        /// Draws the properties of a SerializedObject, mimicking the inspector.
        /// </summary>
        private static void DrawSerializedProperties(SerializedObject serializedObject, Rect rect)
        {
            // Ensure SerializedObject is valid and target hasn't been destroyed
            if (serializedObject == null || serializedObject.targetObject == null)
            {
            EditorGUI.LabelField(rect, "Preview unavailable (Object destroyed).", EditorStyles.centeredGreyMiniLabel);
                return;
            }


            float yOffset = rect.y; // Start drawing from the top of the rect
            float availableWidth = rect.width;

            // It's crucial to Update the serialized object before iterating
            serializedObject.UpdateIfRequiredOrScript();

            var property = serializedObject.GetIterator();
            bool enterChildren = true;


             using (new EditorGUI.DisabledScope(true))
             {
                 if (property.NextVisible(enterChildren))
                 {
                    do
                    {
                        float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
                        var propertyRect = new Rect(rect.x, yOffset, availableWidth, propertyHeight);

                        if (yOffset + propertyHeight > rect.yMax)
                        {
                        // Optionally draw an indicator that not all properties are shown
                        break;
                        }


                        EditorGUI.PropertyField(propertyRect, property, true);

                        // Move Y offset for the next property
                        yOffset += propertyHeight + EditorGUIUtility.standardVerticalSpacing; // Add standard spacing

                    } while (property.NextVisible(false)); // Move to the next visible property at the same level
                 }
             }
        }

        // Kept for potential future use
        private static bool ShouldSkipProperty(string propertyName)
        {
            return propertyName == "m_Script";
        }

        private static void DrawErrorPreview(Rect rect, string errorMessage)
        {
            // Use the provided rect directly
            var style = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.red },
                alignment = TextAnchor.MiddleLeft, // Align text better
                wordWrap = true,
                padding = new RectOffset(4, 4, 4, 4) // Add padding inside the error box
            };
            EditorGUI.LabelField(rect, $"Preview Error: {errorMessage}", style);
        }
    }
}