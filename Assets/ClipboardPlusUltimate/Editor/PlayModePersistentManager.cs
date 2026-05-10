using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ASoliman.Utils.ClipboardPlus
{
    [Serializable]
    public class PlayModePersistenceData : ScriptableObject
    {
        public string componentId;
        public string gameObjectPath;
        public string componentType;
        public string jsonData;
        public Dictionary<string, UnityEngine.Object> referenceMap = new();
        public DateTime captureTime;
    }

    public class PlayModePersistenceManager
    {
        private static PlayModePersistenceManager _instance;
        public static PlayModePersistenceManager Instance => _instance ??= new PlayModePersistenceManager();

        private bool _isRecording;
        private bool _isAutoRecording;
        private readonly List<PlayModePersistenceData> _capturedComponents = new();
        private readonly Dictionary<int, Component> _recordedComponents = new();
        private readonly HashSet<string> _autoRecordedComponentIds = new();
        
        private readonly Dictionary<int, Dictionary<string, UnityEngine.Object>> _lastKnownReferences = new();

        public bool IsRecording => _isRecording;
        public bool IsAutoRecording => _isAutoRecording;
        public IReadOnlyList<PlayModePersistenceData> CapturedComponents => _capturedComponents;

        public bool IsAutoRecordedComponent(string componentId) => _autoRecordedComponentIds.Contains(componentId);

        public void ToggleRecording()
        {
            _isRecording = !_isRecording;
            if (!_isRecording)
            {
                StopAutoRecording();
                ClearCaptures();
            }
            ClipboardEditorWindow.ForceRepaint();
        }

        public void StartAutoRecording(Component component)
        {
            if (!_isRecording || !EditorApplication.isPlaying || component == null)
                return;

            int instanceId = component.GetInstanceID();
            string componentId = instanceId.ToString();

            if (!_recordedComponents.ContainsKey(instanceId))
            {
                _recordedComponents[instanceId] = component;
                _autoRecordedComponentIds.Add(componentId);
                _isAutoRecording = true;
                EditorApplication.update += AutoRecordComponents;
                
                // Initialize reference tracking for this component
                _lastKnownReferences[instanceId] = new Dictionary<string, UnityEngine.Object>();
                CaptureComponentWithReferences(component);
            }
        }

        public void RemoveCapture(string componentId)
        {
            _capturedComponents.RemoveAll(x => x.componentId == componentId);
            _autoRecordedComponentIds.Remove(componentId);
            
            // Clean up reference tracking
            if (int.TryParse(componentId, out int instanceId))
            {
                _lastKnownReferences.Remove(instanceId);
            }
            
            ClipboardEditorWindow.ForceRepaint();
        }

        public void StopAutoRecording(Component component)
        {
            if (component == null) return;
            int instanceId = component.GetInstanceID();
            _recordedComponents.Remove(instanceId);
            _autoRecordedComponentIds.Remove(instanceId.ToString());
            _lastKnownReferences.Remove(instanceId);
            
            if (_recordedComponents.Count == 0)
            {
                _isAutoRecording = false;
                EditorApplication.update -= AutoRecordComponents;
            }
        }

        public void StopAutoRecording()
        {
            _isAutoRecording = false;
            _recordedComponents.Clear();
            _autoRecordedComponentIds.Clear();
            _lastKnownReferences.Clear();
            EditorApplication.update -= AutoRecordComponents;
        }

        private void AutoRecordComponents()
        {
            if (!_isRecording || !_isAutoRecording || !EditorApplication.isPlaying)
            {
                StopAutoRecording();
                return;
            }

            foreach (var component in _recordedComponents.Values.ToList())
            {
                if (component == null)
                {
                    int instanceId = component.GetInstanceID();
                    _recordedComponents.Remove(instanceId);
                    _lastKnownReferences.Remove(instanceId);
                    continue;
                }

                bool hasChanges = false;

                // Check for JSON changes (non-reference fields)
                string currentJson = EditorJsonUtility.ToJson(component);
                var last = _capturedComponents.LastOrDefault(x => x.componentId == component.GetInstanceID().ToString());
                if (last == null || last.jsonData != currentJson)
                {
                    hasChanges = true;
                }

                // Check for reference changes (including scene references)
                if (!hasChanges)
                {
                    int instanceId = component.GetInstanceID();
                    if (_lastKnownReferences.TryGetValue(instanceId, out var lastReferences))
                    {
                        var currentReferences = new Dictionary<string, UnityEngine.Object>();
                        var so = new SerializedObject(component);
                        ExtractObjectReferences(so, "", currentReferences);

                        // Compare current references with last known references
                        foreach (var kvp in currentReferences)
                        {
                            if (!lastReferences.TryGetValue(kvp.Key, out var lastRef) || 
                                !ReferenceEquals(lastRef, kvp.Value))
                            {
                                hasChanges = true;
                                break;
                            }
                        }

                        // Also check if any references were removed
                        if (!hasChanges)
                        {
                            foreach (var kvp in lastReferences)
                            {
                                if (!currentReferences.ContainsKey(kvp.Key))
                                {
                                    hasChanges = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // No previous references recorded, treat as changed
                        hasChanges = true;
                    }
                }

                if (hasChanges)
                {
                    CaptureComponentWithReferences(component);
                }
            }
        }

        private void CaptureComponentWithReferences(Component component)
        {
            // Capture the component state
            CaptureComponent(component);
            
            // Update the reference tracking state
            int instanceId = component.GetInstanceID();
            var references = new Dictionary<string, UnityEngine.Object>();
            var so = new SerializedObject(component);
            ExtractObjectReferences(so, "", references);
            
            // Store the current references for future comparison
            _lastKnownReferences[instanceId] = references;
        }

        public void CaptureComponent(Component component)
        {
            if (!_isRecording || !EditorApplication.isPlaying || component == null)
                return;

            try
            {
                var data = ScriptableObject.CreateInstance<PlayModePersistenceData>();
                data.componentId = component.GetInstanceID().ToString();
                data.gameObjectPath = GetGameObjectPath(component.gameObject);
                data.componentType = component.GetType().AssemblyQualifiedName;
                data.jsonData = EditorJsonUtility.ToJson(component);
                data.captureTime = DateTime.Now;

                // Extract object references just like ClipData
                data.referenceMap = new Dictionary<string, UnityEngine.Object>();
                var so = new SerializedObject(component);
                ExtractObjectReferences(so, "", data.referenceMap);

                _capturedComponents.RemoveAll(x => x.componentId == data.componentId);
                _capturedComponents.Add(data);
                ClipboardEditorWindow.ForceRepaint();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error capturing component state: {e.Message}");
            }
        }

        private void ExtractObjectReferences(SerializedObject serializedObject, string basePath, Dictionary<string, UnityEngine.Object> references)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.Next(enterChildren))
            {
                string path = string.IsNullOrEmpty(basePath) ? property.propertyPath : basePath + "." + property.propertyPath;
                enterChildren = true;

                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        if (property.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            references[path] = property.objectReferenceValue; // even if it's null
                        }
                        break;

                    case SerializedPropertyType.Generic:
                        if (property.isArray)
                        {
                            for (int i = 0; i < property.arraySize; i++)
                            {
                                SerializedProperty element = property.GetArrayElementAtIndex(i);
                                if (element.propertyType == SerializedPropertyType.ObjectReference)
                                {
                                    string elementPath = $"{path}.Array.data[{i}]";
                                    references[elementPath] = element.objectReferenceValue; // even if null
                                }
                            }
                        }
                        break;
                }

                if (property.propertyType == SerializedPropertyType.ArraySize)
                    enterChildren = false;
            }
        }

        public void RestoreComponents()
        {
            if (_capturedComponents.Count == 0) return;

            foreach (var capture in _capturedComponents)
            {
                try
                {
                    RestoreSingleComponent(capture);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error restoring component: {e.Message}");
                }
            }

            ClearCaptures();
        }

        private void RestoreSingleComponent(PlayModePersistenceData capture)
        {
            var targetGO = FindGameObjectByPath(capture.gameObjectPath);
            if (targetGO == null) return;

            var type = Type.GetType(capture.componentType);
            if (type == null) return;

            var component = targetGO.GetComponent(type);
            if (component == null) return;

            Undo.RecordObject(component, "Restore Play Mode Values");
            EditorJsonUtility.FromJsonOverwrite(capture.jsonData, component);

            // Restore object references manually
            SerializedObject serializedObject = new SerializedObject(component);
            foreach (var kvp in capture.referenceMap)
            {
                SerializedProperty prop = serializedObject.FindProperty(kvp.Key);
                if (prop != null && prop.propertyType == SerializedPropertyType.ObjectReference)
                {
                    prop.objectReferenceValue = kvp.Value;
                }
            }
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private string GetGameObjectPath(GameObject go)
        {
            if (go.transform.parent == null)
                return go.name;

            return GetGameObjectPath(go.transform.parent.gameObject) + "/" + go.name;
        }

        private GameObject FindGameObjectByPath(string path)
        {
            return GameObject.Find(path);
        }

        public void ClearCaptures()
        {
            _capturedComponents.Clear();
        }
    }
}
