using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zone8.Events;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// Base class for managing scene loading, dependencies, and bundles.
    /// Provides functionality for loading, unloading, and tracking progress of scene groups.
    /// </summary>
    public abstract class SceneManagementBase : SerializedMonoBehaviour, IProgress<float>
    {
        #region Members

        [Header("Settings")]
        [Tooltip("If true it keeps the first scene as persistant scene")]
        [SerializeField] protected bool _keepPersistantScene = true;

        [Tooltip("The time to wait before updating progress")]
        [SerializeField, Range(.05f, 10)] protected float _progressCheckInterval = 1;

        [Tooltip("Maximum number of retry attempts before failing")]
        [SerializeField, Range(1, 10)] protected int _maxRetries = 2;

        [Tooltip("Maximum time to wait before failing if no progress is made")]
        [SerializeField] protected float _maxIdleTimeInSeconds = 30;

        [Space]
        [Tooltip("List of scene groups available for loading")]
        [SerializeField] protected List<SceneGroup> _sceneGroups;



        protected IAddressableProgressor _addressableProgressor;

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
            _addressableProgressor = GetComponentInChildren<IAddressableProgressor>(true);
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

        public void ClearHandles()
        {
            _sceneDownloadHandler.ReleaseHandles();
        }

        public void ClearHandle(string lable)
        {
            _sceneDownloadHandler.ReleaseHandle(lable);
        }

        /// <summary>
        /// Initiates the loading of a specified scene group.
        /// </summary>
        /// <param name="groupName">The name of the scene group to load.</param>
        /// <param name="relatedBundles">Optional array of related bundle names to load.</param>
        /// <param name="loadProgressor">Progress reporter for scene loading.</param>
        /// <param name="downloadingProgressor">Progress reporter for bundle downloading.</param>
        [Button("Load SceneGroup")]
        public virtual void Load(ESceneGroup groupName, string[] relatedBundles = null,
            IProgress<float> loadProgressor = null, IAddressableProgressor downloadingProgressor = null)
        {
            if (_isLoading) return;
            SceneGroup targetSceneGroup = _sceneGroups.FirstOrDefault(g => g.GroupName == groupName);
            if (targetSceneGroup.GroupName == null)
            {
                Logger.LogError("Invalid scene group name: " + groupName);
                return;
            }

            loadProgressor = loadProgressor == null ? this : loadProgressor;
            downloadingProgressor = downloadingProgressor == null ? _addressableProgressor : downloadingProgressor;

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
            progressor = progressor == null ? _addressableProgressor : progressor;

            foreach (string bundle in relatedBundles)
            {
                if (!await _sceneDownloadHandler.DownloadBundle(bundle, progressor))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual void ReloadCurrentSceneGroup()
        {
            Load(_currentSceneGroup);
        }
        #endregion

        #region Private Members

        protected virtual async void LoadSceneGroup(SceneGroup group, string[] relatedBundles, IProgress<float> sceneLoadProgresseor,
                                          IAddressableProgressor dependancyProgressor)
        {
            _isLoading = true;

            await StartLoadingEffect();

            if (group.GetAddressablesScenesCount() != 0)
            {
                if (!await DownloadSceneGroupDependencies(group, dependancyProgressor))
                {
                    await EndLoadingEffect();
                    _isLoading = false;
                    return;
                }
            }

            if (relatedBundles != null && relatedBundles.Length > 0)
            {
                if (!await DownloadBundles(relatedBundles, dependancyProgressor))
                {
                    await EndLoadingEffect();
                    _isLoading = false;
                    return;
                }
            }

            await _sceneLoadHandler.UnloadScenes();
            ClearHandles();
            await Resources.UnloadUnusedAssets();

            await _sceneLoadHandler.LoadSceneGroup(group, sceneLoadProgresseor);
            await EndLoadingEffect();

            _currentSceneGroup = group.GroupName;
            _isLoading = false;
        }

        protected virtual async Awaitable<bool> DownloadSceneGroupDependencies(SceneGroup group, IAddressableProgressor progressor)
        {
            if (!await _sceneDownloadHandler.DownloadsSceneGroupDependencies(group, progressor))
            {
                return false;
            }
            return true;
        }

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


#if UNITY_EDITOR
        [Button]
        private void ClearCach()
        {
            Caching.ClearCache();
        }
#endif
    }
}
