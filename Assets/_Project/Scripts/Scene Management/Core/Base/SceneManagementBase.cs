using Zone8.Events;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// Base class for managing scene loading, dependencies, and bundles.
    /// Provides functionality for loading, unloading, and tracking progress of scene groups.
    /// </summary>
    public abstract class SceneManagementBase : MonoBehaviour, IProgress<float>
    {
        #region Members

        [Header("Settings")]
        [Tooltip("If true it keeps the first scene as persistant scene")]
        [SerializeField] private bool _keepPersistantScene = true;

        [Tooltip("The time to wait before updating progress")]
        [SerializeField, Range(.05f, 10)] private float _progressCheckInterval = 1;

        [Tooltip("Maximum number of retry attempts before failing")]
        [SerializeField, Range(1, 10)] private int _maxRetries = 2;

        [Tooltip("Maximum time to wait before failing if no progress is made")]
        [SerializeField] private float _maxIdleTimeInSeconds = 30;

        [Space]
        [Tooltip("List of scene groups available for loading")]
        [SerializeField] private List<SceneGroup> _sceneGroups;

        /// <summary>
        /// Handles downloading of scene dependencies and bundles.
        /// </summary>
        protected SceneDownloadHandler _sceneDownloadHandler;

        /// <summary>
        /// Handles loading and unloading of scenes.
        /// </summary>
        protected SceneLoadHandler _sceneLoadHandler;

        /// <summary>
        /// The currently active scene group.
        /// </summary>
        protected ESceneGroup _currentSceneGroup;

        /// <summary>
        /// Indicates whether a scene group is currently being loaded.
        /// </summary>
        protected bool _isLoading;

        #endregion

        /// <summary>
        /// Initializes the scene group manager with the specified settings.
        /// </summary>
        protected virtual void Awake()
        {
            _sceneDownloadHandler = new SceneDownloadHandler(_progressCheckInterval, _maxRetries, _maxIdleTimeInSeconds);
            _sceneLoadHandler = new SceneLoadHandler(_keepPersistantScene);
        }

        #region API Methods

        /// <summary>
        /// Registers a new scene group to the list of available scene groups.
        /// </summary>
        /// <param name="group">The scene group to register.</param>
        public void RegisterSceneGroup(SceneGroup group)
        {
            if (!_sceneGroups.Contains(group))
                _sceneGroups.Add(group);
        }

        /// <summary>
        /// Unregisters a scene group from the list of available scene groups.
        /// </summary>
        /// <param name="group">The scene group to unregister.</param>
        public void UnregisterSceneGroup(SceneGroup group)
        {
            if (_sceneGroups.Contains(group))
                _sceneGroups.Remove(group);
        }

        /// <summary>
        /// Initiates the loading of a specified scene group.
        /// </summary>
        /// <param name="groupName">The name of the scene group to load.</param>
        /// <param name="relatedBundles">Optional array of related bundle names to load.</param>
        /// <param name="loadProgressor">Progress reporter for scene loading.</param>
        /// <param name="downloadingProgressor">Progress reporter for bundle downloading.</param>
        [Button("Load SceneGroup")]
        public void Load(ESceneGroup groupName, string[] relatedBundles = null,
            IProgress<float> loadProgressor = null, IAddressableProgressor downloadingProgressor = null)
        {
            if (_isLoading) return;
            SceneGroup targetSceneGroup = _sceneGroups.FirstOrDefault(g => g.GroupName == groupName);
            if (targetSceneGroup.GroupName == null)
            {
                Logger.LogError("Invalid scene group name: " + groupName);
                return;
            }

            if (loadProgressor == null)
                loadProgressor = this;

            LoadSceneGroup(targetSceneGroup, relatedBundles, loadProgressor, downloadingProgressor);
        }


        /// <summary>
        /// Downloads the specified bundles.
        /// </summary>
        /// <param name="relatedBundles">Array of bundle names to download.</param>
        /// <param name="progressor">Progress reporter for the downloading process.</param>
        /// <returns>A task that resolves to <c>true</c> if all bundles are downloaded successfully; otherwise, <c>false</c>.</returns>
        public virtual async Awaitable<bool> DownloadBundles(string[] relatedBundles,
            IAddressableProgressor progressor)
        {
            if (relatedBundles != null && relatedBundles.Length > 0)
            {
                foreach (string bundle in relatedBundles)
                {
                    if (!await _sceneDownloadHandler.DownloadBundle(bundle, progressor))
                    {
                        EventBus<SceneDownloadingEvent>.Raise(new SceneDownloadingEvent()
                        {
                            Description = $"Failed downloading bundles",
                            Progressor = progressor,
                            State = EDwonloadingState.Failiure,
                        });
                        return false;
                    }
                }
            }

            EventBus<SceneDownloadingEvent>.Raise(new SceneDownloadingEvent()
            {
                Description = $"Downloaded all bundles",
                Progressor = progressor,
                State = EDwonloadingState.Finished,
            });
            return true;
        }

        /// <summary>
        /// Reloads the currently active scene group.
        /// </summary>
        public virtual void ReloadCurrentSceneGroup()
        {
            Load(_currentSceneGroup);
        }
        #endregion

        #region Private Members

        /// <summary>
        /// Manages the scene group loading process.
        /// </summary>
        /// <param name="group">The scene group to load.</param>
        /// <param name="relatedBundles">Optional array of related bundle names to load.</param>
        /// <param name="sceneLoadProgresseor">Progress reporter for scene loading.</param>
        /// <param name="dependancyProgressor">Progress reporter for bundle downloading.</param>
        protected virtual async void LoadSceneGroup(SceneGroup group, string[] relatedBundles, IProgress<float> sceneLoadProgresseor,
                                          IAddressableProgressor dependancyProgressor)
        {
            _isLoading = true;

            if (!await DownloadSceneGroupDependencies(group, dependancyProgressor))
            {
                _isLoading = false;
                return;
            }

            if (!await DownloadBundles(relatedBundles, dependancyProgressor))
            {
                _isLoading = false;
                return;
            }

            await StartLoadingEffect();
            await _sceneLoadHandler.UnloadScenes();
            await _sceneLoadHandler.LoadSceneGroup(group, sceneLoadProgresseor);
            await EndLoadingEffect();

            EventBus<SceneGroupLoadEvent>.Raise(new SceneGroupLoadEvent()
            {
                SceneGroup = group,
                LoadStatues = ESceneLoadStatus.FinishedFadeinEffect
            });

            _currentSceneGroup = group.GroupName;
            _isLoading = false;
        }

        /// <summary>
        /// Checks and downloads dependencies for a scene group.
        /// </summary>
        /// <param name="group">The scene group to check and download dependencies for.</param>
        /// <param name="progressor">Progress reporter for the downloading process.</param>
        /// <returns>A task that resolves to <c>true</c> if all dependencies are downloaded successfully; otherwise, <c>false</c>.</returns>
        protected virtual async Awaitable<bool> DownloadSceneGroupDependencies(SceneGroup group, IAddressableProgressor progressor)
        {
            if (group.GetAddressablesScenesCount() != 0)
            {
                if (!await _sceneDownloadHandler.DownloadsSceneGroupDependencies(group, progressor))
                {
                    EventBus<SceneDownloadingEvent>.Raise(new SceneDownloadingEvent()
                    {
                        Description = $"Failed to download dependencies for {group.GroupName.name} Scene",
                        State = EDwonloadingState.Failiure,
                        Progressor = progressor,
                    });
                    return false;
                }
            }

            EventBus<SceneDownloadingEvent>.Raise(new SceneDownloadingEvent()
            {
                Description = $"Successfully downloaded dependencies for {group.GroupName.name} Scene",
                State = EDwonloadingState.Finished,
                Progressor = progressor,
            });
            return true;
        }

        /// <summary>
        /// Checks if a SceneGroup is registered.
        /// </summary>
        protected bool IsGroupRegistered(ESceneGroup group)
        {
            return _sceneGroups.Any(item => item.GroupName == group);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Reports the progress of the loading process.
        /// </summary>
        /// <param name="loadingProgress">The current progress of the loading process.</param>
        public abstract void Report(float loadingProgress);

        /// <summary>
        /// Starts the loading effect.
        /// </summary>
        /// <returns>An awaitable task that completes when the loading effect starts.</returns>
        public abstract Awaitable StartLoadingEffect();

        /// <summary>
        /// Ends the loading effect.
        /// </summary>
        /// <returns>An awaitable task that completes when the loading effect ends.</returns>
        public abstract Awaitable EndLoadingEffect();

        #endregion
    }
}
