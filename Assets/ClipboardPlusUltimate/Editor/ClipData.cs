using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace ASoliman.Utils.ClipboardPlus
{
/// <summary>
/// Represents a data container for storing and managing clipboard data in the Unity Editor.
/// This ScriptableObject handles serialization and deserialization of various Unity object types
/// for clipboard operations.
/// </summary>
    [Serializable]
    public class ClipData : ScriptableObject
    {
        public string id = Guid.NewGuid().ToString();
        public string sourcePath;
        public string sourceType;
        public DateTime creationDate;
        public bool isFavorite;
        public ClipType clipType;
        public string jsonData; // Store JSON separately for easier debugging
        public Dictionary<string, UnityEngine.Object> referenceMap = new Dictionary<string, UnityEngine.Object>(); // Path -> Object reference
        public Dictionary<string, ReferenceData> referenceMetadata = new Dictionary<string, ReferenceData>();
        [SerializeField] public Dictionary<string, string> additionalData = new Dictionary<string, string>();
        public string componentPath;
        public SerializedPropertyType propertyType;
        public bool isExpanded = false;

        public Type DataType { get; set; }

        /// <summary>
        /// Initializes the ClipData instance with the provided values.
        /// </summary>
        public void Initialize(string id, string sourcePath, string sourceType, DateTime creationDate, Type dataType, 
            ClipType clipType, string jsonData, Dictionary<string, UnityEngine.Object> referenceMap, 
            string componentPath, SerializedPropertyType propertyType)
        {
            this.id = id;
            this.sourcePath = sourcePath;
            this.sourceType = sourceType;
            this.creationDate = creationDate;
            this.DataType = dataType;
            this.clipType = clipType;
            this.jsonData = jsonData;
            this.referenceMap = new Dictionary<string, UnityEngine.Object>(referenceMap ?? new Dictionary<string, UnityEngine.Object>());
            this.componentPath = componentPath;
            this.propertyType = propertyType;
        }


        /// <summary>
        /// Captures and serializes data from a Unity Object based on the specified ClipType.
        /// Currently supports Component type captures.
        /// </summary>
        /// <param name="source">The Unity Object to capture data from</param>
        /// <param name="type">The type of clip to create</param>
        public void CaptureFromObject(UnityEngine.Object source, ClipType type)
        {
            if (source == null) return;

            Undo.RegisterCompleteObjectUndo(this, "Capture Object Data");
            clipType = type;
            DataType = source.GetType();
            creationDate = DateTime.Now;

            try
            {
                switch (type)
                {
                    case ClipType.Component:
                        var component = source as Component;
                        if (component != null)
                        {
                            sourcePath = component.gameObject.name;
                            sourceType = component.GetType().Name;
                            CaptureComponent(component);

                            // Special handling for ParticleSystem
                            HandleSpecialComponents(component);
                        }
                        break;
                }

                EditorUtility.SetDirty(this);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error capturing object data: {e.Message}\n{e.StackTrace}");
                jsonData = null;
                referenceMap.Clear();
                additionalData?.Clear();
            }
        }

        public Dictionary<string, ReferenceData> CollectReferenceMetadata()
        {
            var metadata = new Dictionary<string, ReferenceData>();
            
            foreach (var kvp in referenceMap)
            {
                string propertyPath = kvp.Key;
                UnityEngine.Object refObject = kvp.Value;
                
                if (refObject == null) continue;
                
                var refData = new ReferenceData
                {
                    instanceID = refObject.GetInstanceID(),
                    assetPath = AssetDatabase.GetAssetPath(refObject),
                    name = refObject.name,
                    typeName = refObject.GetType().AssemblyQualifiedName
                };
                
                // Check if this is a scene object (no asset path)
                refData.isSceneReference = string.IsNullOrEmpty(refData.assetPath) && !(refObject is ScriptableObject);
                
                if (refData.isSceneReference)
                {
                    Component component = refObject as Component;
                    GameObject gameObject = refObject as GameObject;
                    
                    if (component != null)
                        gameObject = component.gameObject;
                        
                    if (gameObject != null)
                    {
                        // Create scene reference data
                        var sceneData = new SceneReferenceData
                        {
                            objectName = gameObject.name,
                            hierarchyPath = GetHierarchyPath(gameObject),
                            componentTypeName = refObject.GetType().AssemblyQualifiedName,
                            instanceID = refObject.GetInstanceID(),
                            scenePath = gameObject.scene.path,
                            transformSiblingIndex = gameObject.transform.GetSiblingIndex(),
                            isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(gameObject)
                        };
                        
                        if (sceneData.isPrefabInstance)
                        {
                            var prefabInstanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
                            var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefabInstanceRoot);
                            
                            if (prefabAsset != null)
                            {
                                sceneData.prefabAssetPath = AssetDatabase.GetAssetPath(prefabAsset);
                                sceneData.relativePathInPrefab = GetRelativePathInPrefab(gameObject, prefabInstanceRoot);
                            }
                        }
                        
                        refData.sceneData = sceneData;
                    }
                }
                
                metadata[propertyPath] = refData;
            }
            
            return metadata;
        }
        
        private void CaptureComponent(Component component)
        {
            if (component == null) return;
            
            // Clear existing references
            referenceMap.Clear();
            
            // Create a serialized object of the component to inspect its properties
            SerializedObject serializedObject = new SerializedObject(component);
            
            // First collect all object references
            ExtractObjectReferences(serializedObject, "", referenceMap);
            
            // Store standard JSON for other properties
            jsonData = EditorJsonUtility.ToJson(component);
            
        }
        
        private void ExtractObjectReferences(SerializedObject serializedObject, string basePath, Dictionary<string, UnityEngine.Object> references)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            
            while (property.Next(enterChildren))
            {
                // Create the current property path
                string propertyPath = string.IsNullOrEmpty(basePath) ? property.propertyPath : basePath + "." + property.propertyPath;
                
                enterChildren = true;
                
                // Handle different property types
                switch (property.propertyType)
                {
                    case SerializedPropertyType.ObjectReference:
                        if (property.objectReferenceValue != null)
                        {
                            references[propertyPath] = property.objectReferenceValue;
                        }
                        break;
                        
                    case SerializedPropertyType.Generic:
                        // This might be an array/list, so we need to iterate through its elements
                        if (property.isArray)
                        {
                            for (int i = 0; i < property.arraySize; i++)
                            {
                                SerializedProperty elementProperty = property.GetArrayElementAtIndex(i);
                                if (elementProperty.propertyType == SerializedPropertyType.ObjectReference && 
                                    elementProperty.objectReferenceValue != null)
                                {
                                    string elementPath = $"{propertyPath}.Array.data[{i}]";
                                    references[elementPath] = elementProperty.objectReferenceValue;
                                }
                            }
                        }
                        break;
                }
                
                // Skip array children enumeration after we process the array itself
                if (property.propertyType == SerializedPropertyType.ArraySize)
                {
                    enterChildren = false;
                }
            }
        }

        private string GetHierarchyPath(GameObject obj)
        {
            if (obj == null) return string.Empty;
            
            List<string> path = new List<string>();
            Transform current = obj.transform;
            
            while (current != null)
            {
                path.Add(current.name);
                current = current.parent;
            }
            
            path.Reverse();
            return string.Join("/", path);
        }

        private string GetRelativePathInPrefab(GameObject obj, GameObject prefabRoot)
        {
            if (obj == prefabRoot) return string.Empty;
            
            List<string> pathParts = new List<string>();
            Transform current = obj.transform;
            
            while (current != null && current.gameObject != prefabRoot)
            {
                pathParts.Add(current.name);
                current = current.parent;
            }
            
            pathParts.Reverse();
            return string.Join("/", pathParts);
        }

        #region Special Components Handling
        private void HandleSpecialComponents(Component component)
        {
            if (component is ParticleSystem particleSystem)
            {
                // Get the associated ParticleSystemRenderer component
                ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    // Store the renderer data in the additionalData dictionary
                    if (additionalData == null)
                        additionalData = new Dictionary<string, string>();
                        
                    additionalData["ParticleSystemRenderer"] = EditorJsonUtility.ToJson(renderer);

                    SerializedObject rendererSO = new SerializedObject(renderer);
                    ExtractObjectReferences(rendererSO, "ParticleSystemRenderer", referenceMap);
                }
                
                // Capture ParticleSystem modules with scene references using the public API
                
                // 1. Handle Lights Module
                var lightsModule = particleSystem.lights;
                if (lightsModule.enabled && lightsModule.light != null)
                {
                    referenceMap["ParticleSystem.LightsModule.light"] = lightsModule.light;
                    
                    // Store additional metadata if needed
                    if (additionalData == null)
                        additionalData = new Dictionary<string, string>();
                    additionalData["LightsModuleEnabled"] = "true";
                }
                
                // 2. Handle Sub Emitters Module
                var subEmittersModule = particleSystem.subEmitters;
                if (subEmittersModule.enabled)
                {
                    int subEmitterCount = subEmittersModule.subEmittersCount;
                    
                    for (int i = 0; i < subEmitterCount; i++)
                    {
                        ParticleSystem subEmitterSystem = subEmittersModule.GetSubEmitterSystem(i);
                        if (subEmitterSystem != null)
                        {
                            referenceMap[$"ParticleSystem.SubEmittersModule.{i}"] = subEmitterSystem;
                        }
                    }
                    
                    // Store the count for reconstruction
                    if (additionalData == null)
                        additionalData = new Dictionary<string, string>();
                    additionalData["SubEmittersCount"] = subEmitterCount.ToString();
                    additionalData["SubEmittersModuleEnabled"] = "true";
                }
                
                
                // 4. Handle External Forces (influence mask)
                var externalForcesModule = particleSystem.externalForces;
                if (externalForcesModule.enabled)
                {
                    if (additionalData == null)
                        additionalData = new Dictionary<string, string>();
                    additionalData["ExternalForcesModuleEnabled"] = "true";
                }
            }
        }
        #endregion
    }
}