using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ASoliman.Utils.ClipboardPlus
{
    public class SceneReferenceResolver
    {
        private static SceneReferenceResolver _instance;
        public static SceneReferenceResolver Instance => _instance ??= new SceneReferenceResolver();
        
        private HashSet<ClipData> _clipsNeedingResolution = new HashSet<ClipData>();
        private bool _isResolving = false;
        
        private SceneReferenceResolver()
        {
            EditorApplication.update += CheckPendingResolutions;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        public void RegisterForResolution(ClipData clip)
        {
            if (clip == null) return;
            _clipsNeedingResolution.Add(clip);
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Need to wait until scene is fully loaded
            EditorApplication.delayCall += ScheduleResolution;
        }
        
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                // Wait until play mode state is stable
                EditorApplication.delayCall += ScheduleResolution;
            }
        }
        
        private void ScheduleResolution()
        {
            if (!_isResolving && _clipsNeedingResolution.Count > 0)
            {
                EditorApplication.delayCall += ResolveAllRegisteredClips;
            }
        }
        
        private void CheckPendingResolutions()
        {
            if (_isResolving || _clipsNeedingResolution.Count == 0) return;
            
            // Don't resolve during critical editor operations
            if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
            {
                _isResolving = true;
                EditorApplication.delayCall += ResolveAllRegisteredClips;
            }
        }
        
        private void ResolveAllRegisteredClips()
        {
            // Make a copy of the set to avoid modification during iteration
            var clipsToResolve = _clipsNeedingResolution.ToList();
            _clipsNeedingResolution.Clear();
            
            foreach (var clip in clipsToResolve)
            {
                if (clip != null)
                {
                    ResolveClipReferences(clip);
                }
            }
            
            _isResolving = false;
            
            // Force repaint of any windows showing clips
            ClipboardEditorWindow.ForceRepaint();
        }
        
        private void ResolveClipReferences(ClipData clip)
        {
            var resolvedReferences = new Dictionary<string, UnityEngine.Object>();
            bool anyResolved = false;
            
            if (clip.referenceMetadata == null || clip.referenceMetadata.Count == 0)
                return;
                
            foreach (var kvp in clip.referenceMetadata)
            {
                string propertyPath = kvp.Key;
                ReferenceData refData = kvp.Value;
                
                UnityEngine.Object resolvedObj = null;
                
                if (!refData.isSceneReference)
                {
                    // Asset reference - try to load from path
                    if (!string.IsNullOrEmpty(refData.assetPath))
                    {
                        resolvedObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(refData.assetPath);
                    }
                }
                else if (refData.sceneData != null)
                {
                    // Scene reference - try to resolve
                    resolvedObj = FindSceneObject(refData.sceneData);
                }
                
                if (resolvedObj != null)
                {
                    resolvedReferences[propertyPath] = resolvedObj;
                    anyResolved = true;
                }
            }
            
            if (anyResolved)
            {
                // Apply resolved references to the clip
                foreach (var kvp in resolvedReferences)
                {
                    clip.referenceMap[kvp.Key] = kvp.Value;
                }
            }
        }
        
        private UnityEngine.Object FindSceneObject(SceneReferenceData sceneData)
        {
            GameObject foundObj = null;
            
            // Strategy 1: Try to find by hierarchy path
            if (!string.IsNullOrEmpty(sceneData.hierarchyPath))
            {
                foundObj = FindByHierarchyPath(sceneData.hierarchyPath);
            }
            
            // Strategy 2: If this is a prefab instance, try to find by prefab info
            if (foundObj == null && sceneData.isPrefabInstance && !string.IsNullOrEmpty(sceneData.prefabAssetPath))
            {
                foundObj = FindPrefabInstance(sceneData);
            }
            
            // Strategy 3: Try by name and type
            if (foundObj == null && !string.IsNullOrEmpty(sceneData.objectName))
            {
                foundObj = FindByNameAndType(sceneData);
            }
            
            // If we found a GameObject but need a Component
            if (foundObj != null && sceneData.componentTypeName != typeof(GameObject).AssemblyQualifiedName)
            {
                Type componentType = Type.GetType(sceneData.componentTypeName);
                if (componentType != null)
                {
                    return foundObj.GetComponent(componentType);
                }
            }
            
            return foundObj;
        }
        
        private GameObject FindByHierarchyPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            
            string[] pathParts = path.Split('/');
            if (pathParts.Length == 0) return null;
            
            // Find all root objects with matching name
            List<GameObject> matches = new List<GameObject>();
            
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == pathParts[0])
                    matches.Add(root);
            }
            
            if (matches.Count == 0) return null;
            
            // For single element paths, return the first match
            if (pathParts.Length == 1)
                return matches[0];
                
            // For longer paths, try to follow each match
            foreach (GameObject root in matches)
            {
                GameObject current = root;
                bool success = true;
                
                for (int i = 1; i < pathParts.Length; i++)
                {
                    Transform child = current.transform.Find(pathParts[i]);
                    if (child == null)
                    {
                        success = false;
                        break;
                    }
                    current = child.gameObject;
                }
                
                if (success)
                    return current;
            }
            
            return null;
        }
        
        private GameObject FindPrefabInstance(SceneReferenceData sceneData)
        {
            // Load the prefab asset
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(sceneData.prefabAssetPath);
            if (prefabAsset == null) return null;
            
            // Find all instances of this prefab in the scene
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            List<GameObject> prefabInstances = new List<GameObject>();
            
            foreach (var root in rootObjects)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(root))
                {
                    GameObject prefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(root);
                    GameObject sourceAsset = PrefabUtility.GetCorrespondingObjectFromSource(prefabRoot);
                    
                    if (sourceAsset == prefabAsset)
                    {
                        prefabInstances.Add(prefabRoot);
                    }
                }
            }
            
            if (prefabInstances.Count == 0) return null;
            
            // If no relative path, return the first instance
            if (string.IsNullOrEmpty(sceneData.relativePathInPrefab))
                return prefabInstances[0];
                
            // Try to find the object within the prefab instance
            foreach (var instance in prefabInstances)
            {
                Transform child = instance.transform.Find(sceneData.relativePathInPrefab);
                if (child != null)
                    return child.gameObject;
            }
            
            return null;
        }
        
        private GameObject FindByNameAndType(SceneReferenceData sceneData)
        {
            // Last resort: find object by name
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.name == sceneData.objectName)
                {
                    // If we have component type info, check for component
                    if (sceneData.componentTypeName != typeof(GameObject).AssemblyQualifiedName)
                    {
                        Type componentType = Type.GetType(sceneData.componentTypeName);
                        if (componentType != null && obj.GetComponent(componentType) != null)
                        {
                            return obj;
                        }
                    }
                    else
                    {
                        return obj;
                    }
                }
            }
            
            return null;
        }
    }
}