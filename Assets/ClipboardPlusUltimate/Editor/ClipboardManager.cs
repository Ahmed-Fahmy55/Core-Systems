using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ASoliman.Utils.ClipboardPlus
{
    /// <summary>
    /// Manages a persistent clipboard system for Unity Editor, allowing copying and pasting of component data
    /// across different objects and sessions. Supports undo/redo operations and maintains a history of clips
    /// with optional favorites.
    /// </summary>
    [InitializeOnLoad]
    public class ClipboardManager : EditorWindow
    {   
        private static ClipboardManager _instance;
        public static ClipboardManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<ClipboardManager>();
                    _instance.Initialize();
                }
                return _instance;
            }
        }

        [SerializeField] private List<ClipData> _clips = new List<ClipData>();
        private Dictionary<string, ClipData> _clipLookup = new Dictionary<string, ClipData>();
        private readonly int _maxClips = 100;
        private string _dataPath;
        private string _clipsFilePath;
        private string _referencesDirectoryPath;

        private EditorSceneManager.SceneOpenedCallback _sceneOpenedCallback;

        // List of component types to use direct approach with
        private static readonly HashSet<Type> directApproachTypes = new HashSet<Type>
        {
            typeof(Transform),
            typeof(RectTransform),
            typeof(Rigidbody),
            typeof(Rigidbody2D),
            typeof(Collider),
            typeof(BoxCollider),
            typeof(SphereCollider),
            typeof(CapsuleCollider),
            typeof(MeshCollider),
            typeof(Collider2D),
            typeof(BoxCollider2D),
            typeof(CircleCollider2D),
            typeof(PolygonCollider2D),
            typeof(Camera),
            typeof(AudioListener),
            typeof(CanvasRenderer),
            typeof(Renderer),
            typeof(MeshRenderer),
            typeof(SpriteRenderer),
            typeof(MeshFilter),
            typeof(Animation),
            typeof(UnityEngine.AI.NavMeshAgent),
            typeof(ParticleSystem)
            // Add any other problematic component types
        };

        private void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;
        private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;
        private void OnDestroy()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= SaveClips;
            AssemblyReloadEvents.afterAssemblyReload -= LoadClips;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorSceneManager.sceneOpened -= _sceneOpenedCallback;
        }

        private void Initialize()
        {
            InitializePaths();
            Directory.CreateDirectory(_dataPath);
            LoadClips();
            RebuildLookup();

            AssemblyReloadEvents.beforeAssemblyReload += SaveClips;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _sceneOpenedCallback = (_, _) => LoadClips();
            EditorSceneManager.sceneOpened += _sceneOpenedCallback;
        }

        private void OnAfterAssemblyReload()
        {
            LoadClips();
            
            // Schedule resolution of scene references
            EditorApplication.delayCall += () => {
                foreach (var clip in _clips)
                {
                    if (clip != null && clip.referenceMetadata != null)
                    {
                        SceneReferenceResolver.Instance.RegisterForResolution(clip);
                    }
                }
            };
        }

        private void InitializePaths()
        {
            _dataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Unity",
                "ClipboardPlus",
                PlayerSettings.productGUID.ToString()
            );
            
            _clipsFilePath = Path.Combine(_dataPath, "clipboard_data.json");
            _referencesDirectoryPath = Path.Combine(_dataPath, "References");
        }

        /// <summary>
        /// Adds a new clip to the clipboard history from the specified source object.
        /// </summary>
        /// <param name="source">The source object to create clip from</param>
        /// <param name="clipType">The type of clip to create</param>
        /// <returns>The created ClipData or null if creation fails</returns>
        public ClipData AddClip(UnityEngine.Object source, ClipType clipType)
        {
            if (source == null) return null;

            var clip = CreateInstance<ClipData>();

            try
            {
                Undo.RegisterCompleteObjectUndo(this, "Add Clipboard Item");
                clip.CaptureFromObject(source, clipType);
                
                _clips.Insert(0, clip);
                _clipLookup[clip.id] = clip;

                EnforceClipLimit();
                SaveClips();
                return clip;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating clip: {e.Message}");
                DestroyImmediate(clip);
                return null;
            }
        }

        /// <summary>
        /// Pastes the specified clip data onto the target object.
        /// </summary>
        public void PasteClip(ClipData clip, UnityEngine.Object target)
        {
            if (clip == null || target == null) return;

            Undo.RecordObject(target, "Paste Values");

            switch (clip.clipType)
            {
                case ClipType.Component when target is Component component:
                    if (clip.DataType == component.GetType())
                    {
                        PasteComponent(clip, component);
                    }
                    else
                    {
                        Debug.LogWarning($"Component type mismatch. Clip: {clip.DataType?.Name}, Target: {component.GetType().Name}");
                    }
                    break;
                default:
                    Debug.LogWarning($"Unsupported ClipType for pasting: {clip.clipType}");
                    break;
            }

            EditorUtility.SetDirty(target);
            if (!EditorApplication.isPlaying)
            {
                AssetDatabase.SaveAssets();
            }
        }

        public void RemoveClip(string id, bool registerUndo = true)
        {
            if (string.IsNullOrEmpty(id) || !_clipLookup.TryGetValue(id, out var clip))
                return;

            int index = _clips.IndexOf(clip);
            if (index == -1) return;

            if (registerUndo)
            {
                Undo.RegisterCompleteObjectUndo(this, "Remove Clipboard Item");
            }

            _clips.RemoveAt(index);
            _clipLookup.Remove(id);

            if (!registerUndo && clip != null)
            {
                DestroyImmediate(clip);
            }

            SaveClips();
        }

        public void ValidateClips()
        {
            _clips.RemoveAll(clip => clip == null ||
                                string.IsNullOrEmpty(clip?.id) ||
                                string.IsNullOrEmpty(clip?.jsonData));
            
            RebuildLookup();
        }

        public void SwapClips(string clipId1, string clipId2)
        {
            var allClips = GetClips();
            if (allClips == null) return;
            
            var clip1Index = allClips.FindIndex(c => c.id == clipId1);
            var clip2Index = allClips.FindIndex(c => c.id == clipId2);
            
            if (clip1Index < 0 || clip2Index < 0) return;
            
            // Swap the clips in the list
            var temp = allClips[clip1Index];
            allClips[clip1Index] = allClips[clip2Index];
            allClips[clip2Index] = temp;
        }

        public List<ClipData> GetClips() => _clips;
        
        public ClipData GetRecentClipOfType(Type type) => 
            _clips.FirstOrDefault(c => c.DataType == type);

        public void ClearAllClips()
        {
            var nonFavoriteClips = _clips.Where(c => !c.isFavorite).ToList();
            foreach (var clip in nonFavoriteClips)
            {
                _clips.Remove(clip);
                _clipLookup.Remove(clip.id);
            }
            SaveClips();
        }

        public void ToggleFavorite(string id)
        {
            if (_clipLookup.TryGetValue(id, out var clip))
            {
                clip.isFavorite = !clip.isFavorite;
                SaveClips();
            }
        }

        private void PasteComponent(ClipData clip, Component target)
        {
            if (target == null || clip == null || clip.DataType == null || string.IsNullOrEmpty(clip.jsonData))
            {
                Debug.LogError("PasteComponent failed: Invalid target or clip data.");
                return;
            }

            if (clip.DataType != target.GetType())
            {
                Debug.LogWarning($"PasteComponent skipped: Type mismatch. Clip: {clip.DataType.Name}, Target: {target.GetType().Name}");
                return;
            }

            // Special components handling
            bool handledSpecialComponent = SpecialComponentsHandler(clip, target);
            if (handledSpecialComponent)
            {
                // returns early if a special component was DETECTED (and handled successfully tho)
                return;
            }

            // Check if this component type should use the direct approach
            bool useDirectApproach = directApproachTypes.Contains(clip.DataType);

            if (useDirectApproach)
            {
                // Direct approach with no temporary objects for special types
                Undo.RecordObject(target, $"Paste {clip.DataType.Name} Values");

                // Create a serialized object for the target
                var serializedTarget = new SerializedObject(target);
                serializedTarget.Update();

                // Apply the JSON data directly to the target
                EditorJsonUtility.FromJsonOverwrite(clip.jsonData, target);

                // Apply changes and mark dirty
                serializedTarget.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
                return; // Exit early after handling these special cases
            }

            GameObject temporaryGameObjectToDestroy = null;
            SerializedObject sourceSO = null;
            SerializedObject targetSO = null;
            UnityEngine.Object tempInstanceForCleanup = null;
            CopyStats stats = new CopyStats();

            try
            {
                if (typeof(Component).IsAssignableFrom(clip.DataType) && !clip.DataType.IsAbstract)
                {
                    // Create the temporary GameObject explicitly
                    temporaryGameObjectToDestroy = new GameObject($"__TempPasteSource_{clip.id}") { hideFlags = HideFlags.HideAndDontSave };
                    Component tempComponent = temporaryGameObjectToDestroy.AddComponent(clip.DataType);

                    if (tempComponent != null)
                    {
                        EditorJsonUtility.FromJsonOverwrite(clip.jsonData, tempComponent);
                        sourceSO = new SerializedObject(tempComponent);

                        // Apply references to the temp component's SO
                        if (clip.referenceMap != null)
                        {
                            foreach (var kvp in clip.referenceMap)
                            {
                                SerializedProperty tempProp = sourceSO.FindProperty(kvp.Key);
                                if (tempProp != null && tempProp.propertyType == SerializedPropertyType.ObjectReference)
                                {
                                    // Ensure reference is valid before assigning
                                    if (kvp.Value != null)
                                    {
                                        tempProp.objectReferenceValue = kvp.Value;
                                    }
                                }
                            }
                            sourceSO.ApplyModifiedPropertiesWithoutUndo(); // Apply refs to temp object
                        }
                    }
                    else
                    {
                        // Cleanup immediately if component add failed
                        if (temporaryGameObjectToDestroy != null)
                        {
                            GameObject.DestroyImmediate(temporaryGameObjectToDestroy);
                            temporaryGameObjectToDestroy = null; // Prevent double delete in finally
                        }
                        throw new InvalidOperationException($"Failed to add temporary component of type {clip.DataType.Name}");
                    }
                }
                else if (typeof(ScriptableObject).IsAssignableFrom(clip.DataType))
                {
                    ScriptableObject tempSOInstance = ScriptableObject.CreateInstance(clip.DataType);
                    if (tempSOInstance == null) throw new InvalidOperationException($"Failed to create temporary ScriptableObject instance of type {clip.DataType.Name}");

                    tempInstanceForCleanup = tempSOInstance;
                    EditorJsonUtility.FromJsonOverwrite(clip.jsonData, tempSOInstance);
                    sourceSO = new SerializedObject(tempSOInstance);

                    // Apply references
                    if (clip.referenceMap != null)
                    {
                        foreach (var kvp in clip.referenceMap)
                        {
                            SerializedProperty tempProp = sourceSO.FindProperty(kvp.Key);
                            if (tempProp != null && tempProp.propertyType == SerializedPropertyType.ObjectReference && kvp.Value != null)
                            {
                                tempProp.objectReferenceValue = kvp.Value;
                            }
                        }
                        sourceSO.ApplyModifiedPropertiesWithoutUndo();
                    }
                }
                else
                {
                    throw new NotSupportedException($"Pasting for type {clip.DataType.Name} is not supported by this method.");
                }


                // If creating the source representation failed
                if (sourceSO == null || sourceSO.targetObject == null)
                {
                    throw new InvalidOperationException("Failed to create or initialize source SerializedObject for pasting.");
                }

                targetSO = new SerializedObject(target);
                targetSO.Update();

                Undo.RecordObject(target, $"Paste {clip.DataType.Name} Values");

                SerializedProperty propIterator = sourceSO.GetIterator();

                while (propIterator.NextVisible(true))
                {
                    if (propIterator.propertyPath == "m_Script") continue;
                    if (propIterator.propertyPath == "m_GameObject") continue;
                    if (target is Transform && propIterator.propertyPath == "m_Father") continue;

                    stats.TotalFields++;

                    SerializedProperty targetProp = targetSO.FindProperty(propIterator.propertyPath);
                    if (targetProp != null)
                    {
                        try {
                            // Use CopyFromSerializedProperty for robust copying
                            targetSO.CopyFromSerializedProperty(propIterator);
                            stats.CopiedFields++;
                        }
                        catch (Exception) {
                            stats.FailedFields.Add(propIterator.propertyPath);
                        }
                    }
                    else
                    {
                        stats.FailedFields.Add(propIterator.propertyPath);
                    }
                }

                if (stats.CopiedFields > 0)
                {
                    targetSO.ApplyModifiedProperties();
                    
                    // Show warning if some fields failed to copy
                    if (stats.FailedFields.Count > 0)
                    {
                        ShowCopyWarning(target.GetType().Name, stats);
                    }
                }
                else
                {
                    Debug.LogWarning($"PasteComponent: No properties were copied for clip {clip.id} to target {target.name}.");
                }

                // Mark dirty and handle prefabs
                EditorUtility.SetDirty(target);
                if (PrefabUtility.IsPartOfPrefabInstance(target))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(target);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during PasteComponent for Target: {target?.name} ({target?.GetType().Name}) from Clip: {clip?.id}. Error: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                sourceSO?.Dispose();
                targetSO?.Dispose();

                // Destroy ONLY the explicitly tracked temporary GameObject if it exists
                if (temporaryGameObjectToDestroy != null)
                {
                    GameObject.DestroyImmediate(temporaryGameObjectToDestroy);
                    temporaryGameObjectToDestroy = null; // Clear reference after destruction
                }

                // Destroy the temporary ScriptableObject instance if created
                if (tempInstanceForCleanup != null)
                {
                    ScriptableObject.DestroyImmediate(tempInstanceForCleanup);
                    tempInstanceForCleanup = null;
                }
            }
        }

        /// <summary>
        /// Ensures the clip list doesn't exceed the maximum allowed clips by removing oldest non-favorite clips.
        /// </summary>
        private void EnforceClipLimit()
        {
            if (_clips.Count > _maxClips)
            {
                var toRemove = _clips.Where(c => !c.isFavorite)
                                .Skip(_maxClips)
                                .ToList();
                foreach (var removeClip in toRemove)
                {
                    RemoveClip(removeClip.id, true);
                }
            }
        }

        private void SaveClips()
        {
            try
            {
                // Ensure directories exist
                Directory.CreateDirectory(_dataPath);
                Directory.CreateDirectory(_referencesDirectoryPath);
                
                foreach (var clip in _clips)
                {
                    if (clip == null) continue;
                    
                    // Collect metadata for references (especially scene references)
                    var referenceMetadata = clip.CollectReferenceMetadata();
                    
                    // Store reference metadata in clip
                    clip.referenceMetadata = referenceMetadata;
                    
                    // Save reference metadata to local app data
                    string clipReferencesDir = Path.Combine(_referencesDirectoryPath, clip.id);
                    Directory.CreateDirectory(clipReferencesDir);
                    
                    // Serialize reference metadata
                    string referencesJson = JsonUtility.ToJson(new ReferenceDataWrapper { references = referenceMetadata }, true);
                    File.WriteAllText(Path.Combine(clipReferencesDir, "references.json"), referencesJson);
                }
                
                // Save clip list data
                var serializedData = new ClipboardData
                {
                    clips = _clips.Select(clip => new SerializedClipData
                    {
                        id = clip.id,
                        sourcePath = clip.sourcePath,
                        sourceType = clip.sourceType,
                        creationDate = clip.creationDate.ToString("o"),
                        dataTypeName = clip.DataType?.AssemblyQualifiedName,
                        clipType = clip.clipType,
                        jsonData = clip.jsonData,
                        componentPath = clip.componentPath,
                        propertyType = clip.propertyType,
                        isFavorite = clip.isFavorite,
                        isExpanded = clip.isExpanded,
                        referenceCount = clip.referenceMap.Count > 0 ? clip.referenceMap.Count : 0,
                        additionalDataJson = clip.additionalData != null && clip.additionalData.Count > 0 ? 
                            JsonUtility.ToJson(new StringDictWrapper { entries = clip.additionalData
                                .Select(kvp => new StringEntry { key = kvp.Key, value = kvp.Value })
                                .ToList() }) 
                            : null
                    }).ToList()
                };

                string json = JsonUtility.ToJson(serializedData, true);
                File.WriteAllText(_clipsFilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving clipboard data: {e.Message}\n{e.StackTrace}");
            }
        }

        private void LoadClips()
        {
            try
            {
                if (!File.Exists(_clipsFilePath))
                {
                    _clips.Clear();
                    return;
                }

                string json = File.ReadAllText(_clipsFilePath);
                var data = JsonUtility.FromJson<ClipboardData>(json);

                _clips.Clear();
                
                foreach (var serializedClip in data.clips)
                {
                    // Create a new clip instance
                    var clip = CreateInstance<ClipData>();
                    clip.id = serializedClip.id;
                    
                    Type dataType = !string.IsNullOrEmpty(serializedClip.dataTypeName) 
                        ? Type.GetType(serializedClip.dataTypeName) 
                        : null;

                    DateTime creationDate = DateTime.TryParse(serializedClip.creationDate, out var parsed)
                        ? parsed
                        : DateTime.Now;

                    // Create empty reference map if needed
                    if (clip.referenceMap == null)
                        clip.referenceMap = new Dictionary<string, UnityEngine.Object>();
                    
                    // Try to load references metadata
                    string clipReferencesDir = Path.Combine(_referencesDirectoryPath, serializedClip.id);
                    string referencesFilePath = Path.Combine(clipReferencesDir, "references.json");
                    
                    if (File.Exists(referencesFilePath))
                    {
                        try
                        {
                            string referencesJson = File.ReadAllText(referencesFilePath);
                            var refDataWrapper = JsonUtility.FromJson<ReferenceDataWrapper>(referencesJson);
                            
                            if (refDataWrapper != null && refDataWrapper.references != null)
                            {
                                // Store metadata for later resolution
                                clip.referenceMetadata = refDataWrapper.references;
                                
                                // Try immediate resolution of asset references
                                foreach (var kvp in refDataWrapper.references)
                                {
                                    string propertyPath = kvp.Key;
                                    ReferenceData refData = kvp.Value;
                                    
                                    // Only try to resolve assets here, scene objects will be resolved later
                                    if (!refData.isSceneReference && !string.IsNullOrEmpty(refData.assetPath))
                                    {
                                        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(refData.assetPath);
                                        if (obj != null)
                                        {
                                            clip.referenceMap[propertyPath] = obj;
                                        }
                                    }
                                }
                                
                                // Register for scene reference resolution
                                SceneReferenceResolver.Instance.RegisterForResolution(clip);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error loading references for clip {serializedClip.id}: {e.Message}");
                        }
                    }

                    // Set clip properties
                    clip.sourcePath = serializedClip.sourcePath;
                    clip.sourceType = serializedClip.sourceType;
                    clip.DataType = dataType;
                    clip.clipType = serializedClip.clipType;
                    clip.jsonData = serializedClip.jsonData;
                    clip.componentPath = serializedClip.componentPath;
                    clip.propertyType = serializedClip.propertyType;
                    clip.isFavorite = serializedClip.isFavorite;
                    clip.isExpanded = serializedClip.isExpanded;
                    clip.creationDate = creationDate;
                    // Load additional data if any (for comparing copied fields with actual fields of a component, to show warnings if needed)
                    if (!string.IsNullOrEmpty(serializedClip.additionalDataJson))
                    {
                        var wrapper = JsonUtility.FromJson<StringDictWrapper>(serializedClip.additionalDataJson);
                        if (wrapper != null && wrapper.entries != null)
                        {
                            clip.additionalData = new Dictionary<string, string>();
                            foreach (var entry in wrapper.entries)
                            {
                                clip.additionalData[entry.key] = entry.value;
                            }
                        }
                    }
                    
                    _clips.Add(clip);
                }

                RebuildLookup();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading clipboard data: {e.Message}\n{e.StackTrace}");
                _clips.Clear();
            }
        }
        
        private void OnUndoRedo()
        {
            try
            {
                ValidateClips();
                ClipboardEditorWindow.ForceRepaint();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during undo/redo: {e.Message}");
            }
        }

        private void RebuildLookup()
        {
            _clipLookup.Clear();
            foreach (var clip in _clips.Where(c => c != null && !string.IsNullOrEmpty(c.id)))
            {
                if (!_clipLookup.ContainsKey(clip.id))
                {
                    _clipLookup[clip.id] = clip;
                }
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    SaveClips();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    LoadClips();
                    PlayModePersistenceManager.Instance.RestoreComponents();
                    ClipboardEditorWindow.ForceRepaint();
                    break;
            }
        }

        private void ShowCopyWarning(string componentName, CopyStats stats)
        {
            // Only show warning if some fields failed but others succeeded
            if (stats.FailedFields.Count > 0 && stats.CopiedFields > 0)
            {
                // Queue the dialog to display after current operation completes
                EditorApplication.delayCall += () =>
                {
                    int failedCount = stats.FailedFields.Count;
                    int totalCount = stats.TotalFields;
                    
                    string failedFieldsList = string.Join(", ", 
                        stats.FailedFields.Take(5).Select(f => ObjectNames.NicifyVariableName(f.Replace("m_", ""))));
                    
                    if (stats.FailedFields.Count > 5)
                    {
                        failedFieldsList += $", and {stats.FailedFields.Count - 5} more";
                    }
                    
                    bool result = EditorUtility.DisplayDialog(
                        $"{componentName} Copy Warning",
                        $"{failedCount} out of {totalCount} fields could not be copied.\n\n" +
                        $"Failed fields: {failedFieldsList}",
                        "OK"
                    );
                };
            }
        }

        #region Special Components Handling
        private bool SpecialComponentsHandler(ClipData clip, Component target)
        {
            return HandleParticleSystemCopy(clip, target);
        }

        private bool HandleParticleSystemCopy(ClipData clip, Component target)
        {
            if (target is ParticleSystem particleSystem)
            {
                Undo.RecordObject(target, $"Paste {clip.DataType.Name} Values");

                // Apply the main ParticleSystem data
                EditorJsonUtility.FromJsonOverwrite(clip.jsonData, target);

                // Now handle the ParticleSystemRenderer component
                if (clip.additionalData != null && 
                clip.additionalData.TryGetValue("ParticleSystemRenderer", out string rendererJson))
                {
                    ParticleSystemRenderer targetRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                    if (targetRenderer != null)
                    {
                        Undo.RecordObject(targetRenderer, "Paste ParticleSystemRenderer Values");
                        
                        // First apply base values
                        EditorJsonUtility.FromJsonOverwrite(rendererJson, targetRenderer);
                        
                        // Then restore object references
                        SerializedObject targetRendererSO = new SerializedObject(targetRenderer);
                        targetRendererSO.Update();
                        
                        // Apply references from the clip's reference map
                        foreach (var kvp in clip.referenceMap)
                        {
                            // Check if this reference belongs to the renderer
                            if (kvp.Key.StartsWith("ParticleSystemRenderer."))
                            {
                                string actualPropertyPath = kvp.Key.Substring("ParticleSystemRenderer.".Length);
                                SerializedProperty targetProp = targetRendererSO.FindProperty(actualPropertyPath);
                                
                                if (targetProp != null && targetProp.propertyType == SerializedPropertyType.ObjectReference)
                                {
                                    targetProp.objectReferenceValue = kvp.Value;
                                }
                            }
                        }
                        
                        targetRendererSO.ApplyModifiedProperties();
                        EditorUtility.SetDirty(targetRenderer);
                    }
                }

                // Handle ParticleSystem module object references using the public API
                bool needsEditorRefresh = false;
                
                // 1. Handle Lights Module reference
                if (clip.additionalData != null && clip.additionalData.TryGetValue("LightsModuleEnabled", out _) &&
                    clip.referenceMap.TryGetValue("ParticleSystem.LightsModule.light", out UnityEngine.Object lightObj))
                {
                    var lightsModule = particleSystem.lights;
                    Light lightComponent = lightObj as Light;
                    
                    if (lightComponent != null)
                    {
                        // Record the module for undo
                        ParticleSystem.LightsModule moduleCopy = lightsModule;
                        Undo.RecordObject(particleSystem, "Paste Lights Module");
                        
                        // Enable the module first
                        lightsModule.enabled = true;
                        
                        // Set the light reference
                        lightsModule.light = lightComponent;
                        
                        needsEditorRefresh = true;
                    }
                }
                
                // 2. Handle Sub Emitters references
                if (clip.additionalData != null && clip.additionalData.TryGetValue("SubEmittersModuleEnabled", out _) &&
                    clip.additionalData.TryGetValue("SubEmittersCount", out string countStr) && 
                    int.TryParse(countStr, out int subEmitterCount))
                {
                    var subEmittersModule = particleSystem.subEmitters;
                    Undo.RecordObject(particleSystem, "Paste SubEmitters Module");
                    
                    // Enable the module
                    subEmittersModule.enabled = true;
                    
                    // Process each sub emitter
                    for (int i = 0; i < subEmitterCount; i++)
                    {
                        if (clip.referenceMap.TryGetValue($"ParticleSystem.SubEmittersModule.{i}", out UnityEngine.Object subEmitterObj))
                        {
                            ParticleSystem subEmitterPS = subEmitterObj as ParticleSystem;
                            if (subEmitterPS != null && i < subEmittersModule.subEmittersCount)
                            {
                                // If we have a corresponding sub emitter, set its reference
                                // Note: We need to be careful as we can't directly set the sub emitter systems
                                // Instead, we use the property and Type.SetValue() if needed
                                
                                // This is a workaround since Unity doesn't expose direct setting of sub emitters
                                // We'll use reflection to access the property if the direct API doesn't work
                                try
                                {
                                    // First try using the public API to set the property if it's already there
                                    if (i < subEmittersModule.subEmittersCount)
                                    {
                                        var emitType = subEmittersModule.GetSubEmitterType(i);
                                        var properties = subEmittersModule.GetSubEmitterProperties(i);
                                        
                                        // Remove the old sub emitter and add the new one
                                        subEmittersModule.RemoveSubEmitter(i);
                                        subEmittersModule.AddSubEmitter(subEmitterPS, emitType, properties);
                                    }
                                    else
                                    {
                                        // Add a new sub emitter with default settings
                                        subEmittersModule.AddSubEmitter(subEmitterPS, ParticleSystemSubEmitterType.Birth, ParticleSystemSubEmitterProperties.InheritNothing);
                                    }
                                    
                                    needsEditorRefresh = true;
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning($"Failed to set sub emitter {i}: {e.Message}");
                                }
                            }
                        }
                    }
                }
                
                // Make sure Unity recognizes the changes
                if (needsEditorRefresh)
                {
                    EditorUtility.SetDirty(particleSystem);
                    
                    // Request update of the ParticleSystem UI
                    if (EditorApplication.isPlaying)
                        particleSystem.Play();
                    else
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
                
                return true;
            }

            return false;
        }
        #endregion
        
        [Serializable]
        private class ClipboardData
        {
            public List<SerializedClipData> clips = new List<SerializedClipData>();
        }

        [Serializable]
        private class SerializedClipData
        {
            public string id;
            public string sourcePath;
            public string sourceType;
            public string creationDate;
            public string dataTypeName;
            public ClipType clipType;
            public string jsonData;
            public string componentPath;
            public SerializedPropertyType propertyType;
            public bool isFavorite;
            public bool isExpanded;
            public int referenceCount;
            public string additionalDataJson;
        }
        
        // Copy stats for tracking copied fields and failures
        private class CopyStats
        {
            public int TotalFields = 0;
            public int CopiedFields = 0;
            public List<string> FailedFields = new List<string>();
        }

        [Serializable]
        private class StringEntry
        {
            public string key;
            public string value;
        }

        [Serializable]
        private class StringDictWrapper
        {
            public List<StringEntry> entries = new List<StringEntry>();
        }
    }

    [Serializable]
    public class SceneReferenceData
    {
        public string objectName;
        public string hierarchyPath;
        public string componentTypeName;
        public int instanceID;
        public string scenePath;
        public bool isPrefabInstance;
        public string prefabAssetPath;
        public string relativePathInPrefab;
        public int transformSiblingIndex;
    }
        
    [Serializable]
    public class ReferenceData
    {
        public int instanceID;
        public string assetPath;
        public string name;
        public string typeName;
        public bool isSceneReference;
        public SceneReferenceData sceneData;
    }
    
    [Serializable]
    public class ReferenceDataWrapper
    {
        public List<ReferenceDataEntry> referenceList = new List<ReferenceDataEntry>();
        
        public Dictionary<string, ReferenceData> references
        {
            get
            {
                var dict = new Dictionary<string, ReferenceData>();
                foreach (var entry in referenceList)
                {
                    dict[entry.key] = entry.value;
                }
                return dict;
            }
            set
            {
                referenceList.Clear();
                foreach (var kvp in value)
                {
                    referenceList.Add(new ReferenceDataEntry { key = kvp.Key, value = kvp.Value });
                }
            }
        }
    }

    [Serializable]
    public class ReferenceDataEntry
    {
        public string key;
        public ReferenceData value;
    }
}