using Zone8.Events;
using Eflatun.SceneReference;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Zone8.SceneManagement
{
    // Event triggered when a scene group is loaded or unloaded
    public struct SceneGroupLoadEvent : IEvent
    {
        public IProgress<float> Progressor; // Progress reporter for the event
        public SceneGroup SceneGroup; // The scene group involved in the event
        public ESceneLoadStatus LoadStatues; // The status of the load/unload process
    }

    // Event triggered when an individual scene is loaded or unloaded
    public struct SceneLoadEvent : IEvent
    {
        public IProgress<float> Progressor; // Progress reporter for the event
        public SceneData SceneData; // The data of the scene loaded
        public ESceneLoadStatus LoadStatues; // The status of the load/unload process
    }

    // Enum representing the various statuses during scene loading/unloading
    public enum ESceneLoadStatus
    {
        None, Started, Loading, Unloading, FinishedLoading, FinishedUnloading, FinishedFadeinEffect, Error,
    }


    /// <summary>
    /// Handles the loading and unloading of scenes, including both regular and addressable scenes.
    /// Manages scene groups and raises events to notify about the progress and status of scene operations.
    /// </summary>
    public class SceneLoadHandler
    {
        #region Members

        /// <summary>
        /// Tracks the handles of loaded addressable scenes.
        /// </summary>
        private readonly AsyncOperationHandleGroup _loadedGroupHandles = new(10);

        private bool _keepPersistant;

        private const string k_tempEmptySceneName = "TempEmptyScene";
        /// <summary>
        /// Represents the currently active group of scenes.
        /// </summary>
        private SceneGroup _activeSceneGroup;


        public SceneLoadHandler(bool keepPersistant)
        {
            _keepPersistant = keepPersistant;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Loads a group of scenes. This method should be called after downloading the required dependencies.
        /// </summary>
        /// <param name="group">The target scene group to load.</param>
        /// <param name="progressor">Optional progress reporter to track the loading progress.</param>
        /// <returns>An awaitable task that completes when all scenes in the group are loaded.</returns>
        public async Awaitable LoadSceneGroup(SceneGroup group, IProgress<float> progressor)
        {
            _activeSceneGroup = group;

            // Notify that the scene group loading has started
            EventBus<SceneGroupLoadEvent>.Raise(new SceneGroupLoadEvent()
            {
                SceneGroup = group,
                LoadStatues = ESceneLoadStatus.Started,
                Progressor = progressor
            });

            var totalScenesToLoad = _activeSceneGroup.Scenes.Count;
            var operationGroup = new AsyncOperationGroup(totalScenesToLoad);

            // Load each scene in the group
            for (var i = 0; i < totalScenesToLoad; i++)
            {
                var sceneData = group.Scenes[i];

                // Notify that the individual scene loading has started
                EventBus<SceneLoadEvent>.Raise(new SceneLoadEvent()
                {
                    SceneData = sceneData,
                    LoadStatues = ESceneLoadStatus.Started,
                    Progressor = progressor
                });

                if (sceneData.Scene.State == SceneReferenceState.Regular)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneData.Path, LoadSceneMode.Additive);
                    operationGroup.Operations.Add(operation);
                    operation.completed += handle => OnSceneLoad(sceneData, progressor);
                }
                else if (sceneData.Scene.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneData.Scene.Address, LoadSceneMode.Additive);
                    _loadedGroupHandles.Handles.Add(sceneHandle);
                    sceneHandle.Completed += handle => OnSceneLoad(handle, sceneData, progressor);
                }
            }

            // Wait for all scenes to finish loading
            float loadingScenesProgress = 0;
            while (!operationGroup.IsDone || !_loadedGroupHandles.IsDone)
            {
                loadingScenesProgress = operationGroup.Progress + _loadedGroupHandles.Progress;

                if (operationGroup.Operations.Count > 0 && _loadedGroupHandles.Handles.Count > 0)
                {
                    progressor?.Report(loadingScenesProgress / 2);
                }
                else
                {
                    progressor?.Report(loadingScenesProgress);
                }
                await Awaitable.NextFrameAsync();
            }

            // Set the active scene
            string targetActiveScene = _activeSceneGroup.FindScenePathByType(ESceneType.ActiveScene);
            if (targetActiveScene == null)
            {
                Logger.LogError("Couldn't find active scene in the group");
                targetActiveScene = group.Scenes[0].Scene.Path;
            }

            Scene activeScene = SceneManager.GetSceneByPath(targetActiveScene);
            if (activeScene.isLoaded)
            {
                SceneManager.SetActiveScene(activeScene);
            }

            Scene tempScene = SceneManager.GetSceneByName("TempEmptyScene");
            if (tempScene.IsValid() && tempScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(tempScene);
            }

            // Notify that the scene group loading has finished
            EventBus<SceneGroupLoadEvent>.Raise(new SceneGroupLoadEvent()
            {
                SceneGroup = group,
                LoadStatues = ESceneLoadStatus.FinishedLoading,
                Progressor = progressor
            });
        }

        /// <summary>
        /// Unloads all scenes except the main one and releases associated resources.
        /// </summary>
        /// <returns>An awaitable task that completes when all scenes are unloaded.</returns>
        public async Awaitable UnloadScenes()
        {

            var scenes = new List<string>();
            int sceneCount = SceneManager.sceneCount;

            // Collect all scenes to unload except the main one
            for (var i = sceneCount - 1; i >= 0; i--)
            {
                Scene sceneAt = SceneManager.GetSceneAt(i);
                if (!sceneAt.isLoaded) continue;

                var scenePath = sceneAt.path;

                // Skip scenes already tracked in _loadedGroupHandles
                if (_loadedGroupHandles.Handles.Any(h => h.IsValid() && h.Result.Scene.path == scenePath)) continue;

                scenes.Add(scenePath);
            }

            if (!_keepPersistant)
            {
                var emptyScene = SceneManager.CreateScene(k_tempEmptySceneName);
                SceneManager.SetActiveScene(emptyScene);
            }

            // Group to manage async unloading operations
            var operationGroup = new AsyncOperationGroup(scenes.Count);

            foreach (var scene in scenes)
            {
                var operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null) continue;

                operationGroup.Operations.Add(operation);
            }

            // Group to manage Addressables scene unload handles
            AsyncOperationHandleGroup unloadHandleGroup = new(10);

            foreach (var loadedHandle in _loadedGroupHandles.Handles)
            {
                if (loadedHandle.IsValid())
                {
                    var unloadHandle = Addressables.UnloadSceneAsync(loadedHandle);
                    unloadHandleGroup.Handles.Add(unloadHandle);
                }
            }

            // Wait until all unload operations are complete
            while (!operationGroup.IsDone || !unloadHandleGroup.IsDone)
            {
                await Awaitable.WaitForSecondsAsync(.1f); // Delay to prevent a tight loop
            }

            // Clear all tracking lists
            _loadedGroupHandles.Handles.Clear();

            // Notify that the scene unloading has finished
            EventBus<SceneGroupLoadEvent>.Raise(new SceneGroupLoadEvent()
            {
                SceneGroup = _activeSceneGroup,
                LoadStatues = ESceneLoadStatus.FinishedUnloading
            });
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the completion of an addressable scene load operation.
        /// </summary>
        /// <param name="handle">The handle of the completed operation.</param>
        /// <param name="sceneName">The data of the loaded scene.</param>
        /// <param name="progressor">Optional progress reporter.</param>
        private void OnSceneLoad(AsyncOperationHandle<SceneInstance> handle, SceneData sceneName, IProgress<float> progressor)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                EventBus<SceneLoadEvent>.Raise(new SceneLoadEvent()
                {
                    SceneData = sceneName,
                    LoadStatues = ESceneLoadStatus.FinishedLoading,
                    Progressor = progressor
                });
            }
            else
            {
                EventBus<SceneLoadEvent>.Raise(new SceneLoadEvent()
                {
                    SceneData = sceneName,
                    LoadStatues = ESceneLoadStatus.Error,
                    Progressor = progressor
                });
                Logger.LogError($"Failed to load addressable scene: {sceneName.Path}");
            }
        }

        /// <summary>
        /// Handles the completion of a regular scene load operation.
        /// </summary>
        /// <param name="sceneName">The data of the loaded scene.</param>
        /// <param name="progressor">Optional progress reporter.</param>
        private void OnSceneLoad(SceneData sceneName, IProgress<float> progressor)
        {
            EventBus<SceneLoadEvent>.Raise(new SceneLoadEvent()
            {
                SceneData = sceneName,
                LoadStatues = ESceneLoadStatus.FinishedLoading,
                Progressor = progressor
            });
        }

        #endregion
    }

    /// <summary>
    /// Represents a group of asynchronous operations.
    /// </summary>
    public readonly struct AsyncOperationGroup
    {
        /// <summary>
        /// The list of asynchronous operations in the group.
        /// </summary>
        public readonly List<AsyncOperation> Operations;

        /// <summary>
        /// The overall progress of the group.
        /// </summary>
        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);

        /// <summary>
        /// Indicates whether all operations in the group are complete.
        /// </summary>
        public bool IsDone => Operations.All(o => o.isDone);

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncOperationGroup"/> struct.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the operations list.</param>
        public AsyncOperationGroup(int initialCapacity)
        {
            Operations = new List<AsyncOperation>(initialCapacity);
        }
    }
}
