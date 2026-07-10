using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Zone8.Events;
using Zone8.Fading;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// Orchestrates scene-group transitions: fade out, download dependencies
    /// (via <see cref="SceneDownloadHandler"/>), swap the loaded scenes
    /// (via <see cref="SceneLoadHandler"/>), fade back in — raising the
    /// <see cref="SceneTransitionEvent"/> lifecycle along the way.
    /// </summary>
    public class SceneManagementManager : SerializedMonoBehaviour, ISceneManager
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

        [Header("Startup")]
        [SerializeField] private ESceneGroup defaultScene;
        [SerializeField] private bool loadDefaultSceneOnStart;

        private IFader _fader;
        private IAddressableProgressor _addressableProgressor;
        private SceneDownloadHandler _sceneDownloadHandler;
        private SceneLoadHandler _sceneLoadHandler;
        private ESceneGroup _currentSceneGroup;
        private bool _isLoading;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _sceneDownloadHandler = new SceneDownloadHandler(_progressCheckInterval, _maxRetries, _maxIdleTimeInSeconds);
            _sceneLoadHandler = new SceneLoadHandler(_keepPersistantScene);
            _addressableProgressor = GetComponentInChildren<IAddressableProgressor>(true);

            _fader = GetComponentInChildren<IFader>();
            if (_fader == null)
            {
                Logger.LogError("No IFader component found in children of SceneManagementManager. Please add one to enable fading effects.");
            }
        }

        private void Start()
        {
            if (loadDefaultSceneOnStart) LoadDefaultScene();
        }

        #endregion

        #region Public API

        [Button]
        public void LoadDefaultScene()
        {
            _ = Load(defaultScene);
        }

        /// <inheritdoc />
        [Button("Load SceneGroup")]
        public async Awaitable<bool> Load(ESceneGroup groupName, string[] relatedBundles = null,
            IProgress<float> loadProgressor = null, IAddressableProgressor downloadingProgressor = null)
        {
            if (_isLoading)
            {
                Logger.LogWarning($"[SceneManagement] Ignoring load of '{groupName}': another load is in progress.");
                return false;
            }

            if (!TryGetGroup(groupName, out SceneGroup targetSceneGroup))
            {
                Logger.LogError("Invalid scene group name: " + groupName);
                return false;
            }

            downloadingProgressor ??= _addressableProgressor;

            return await LoadSceneGroup(targetSceneGroup, relatedBundles, loadProgressor, downloadingProgressor);
        }

        /// <inheritdoc />
        public async Awaitable<bool> DownloadBundles(string[] relatedBundles, IAddressableProgressor progressor = null)
        {
            progressor ??= _addressableProgressor;

            foreach (string bundle in relatedBundles)
            {
                if (!await _sceneDownloadHandler.DownloadBundle(bundle, progressor))
                {
                    return false;
                }
            }
            return true;
        }

        public Awaitable<bool> ReloadCurrentSceneGroup() => Load(_currentSceneGroup);

        /// <summary>
        /// Resolves a registered scene group from its asset name — for callers that can only
        /// carry a string identity (e.g. a group name replicated over the network).
        /// </summary>
        public bool TryGetGroupByName(string groupAssetName, out ESceneGroup group)
        {
            foreach (var g in _sceneGroups)
            {
                if (g.GroupName != null && g.GroupName.name == groupAssetName)
                {
                    group = g.GroupName;
                    return true;
                }
            }
            group = null;
            return false;
        }

        /// <summary>Registers a scene group at runtime (groups are normally authored in the inspector).</summary>
        public void RegisterSceneGroup(SceneGroup group)
        {
            if (!_sceneGroups.Contains(group))
                _sceneGroups.Add(group);
        }

        /// <summary>Unregisters a runtime-registered scene group.</summary>
        public void UnregisterSceneGroup(SceneGroup group)
        {
            _sceneGroups.Remove(group);
        }

        public void ClearHandles() => _sceneDownloadHandler.ReleaseHandles();

        public void ClearHandle(string label) => _sceneDownloadHandler.ReleaseHandle(label);

        #endregion

        #region Transition

        private async Awaitable<bool> LoadSceneGroup(SceneGroup group, string[] relatedBundles,
            IProgress<float> sceneLoadProgressor, IAddressableProgressor dependencyProgressor)
        {
            _isLoading = true;
            RaiseTransition(group.GroupName, ESceneTransitionPhase.Started);

            try
            {
                await FadeIn();

                bool hasAddressableScenes = group.GetAddressablesScenesCount() != 0;
                bool hasRelatedBundles = relatedBundles is { Length: > 0 };

                if (hasAddressableScenes || hasRelatedBundles)
                {
                    RaiseTransition(group.GroupName, ESceneTransitionPhase.Downloading);

                    if (hasAddressableScenes
                        && !await _sceneDownloadHandler.DownloadSceneGroupDependencies(group, dependencyProgressor))
                    {
                        RaiseTransition(group.GroupName, ESceneTransitionPhase.Failed, "Failed to download scene group dependencies.");
                        return false;
                    }

                    if (hasRelatedBundles
                        && !await DownloadBundles(relatedBundles, dependencyProgressor))
                    {
                        RaiseTransition(group.GroupName, ESceneTransitionPhase.Failed, "Failed to download related bundles.");
                        return false;
                    }
                }

                RaiseTransition(group.GroupName, ESceneTransitionPhase.Unloading);
                await _sceneLoadHandler.UnloadScenes();
                ClearHandles();
                await Resources.UnloadUnusedAssets();

                RaiseTransition(group.GroupName, ESceneTransitionPhase.Loading);
                await _sceneLoadHandler.LoadSceneGroup(group, sceneLoadProgressor);
                _currentSceneGroup = group.GroupName;

                RaiseTransition(group.GroupName, ESceneTransitionPhase.Completed);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"[SceneManagement] Failed to load scene group '{group.GroupName}': {ex}");
                RaiseTransition(group.GroupName, ESceneTransitionPhase.Failed, ex.Message);
                return false;
            }
            finally
            {
                await FadeOut();
                _isLoading = false;
            }
        }

        private async Awaitable FadeIn()
        {
            if (_fader != null) await _fader.FadeIn();
        }

        private async Awaitable FadeOut()
        {
            if (_fader != null) await _fader.FadeOut();
        }

        private bool TryGetGroup(ESceneGroup groupName, out SceneGroup group)
        {
            group = _sceneGroups.FirstOrDefault(g => g.GroupName == groupName);
            return group.GroupName != null;
        }

        private static void RaiseTransition(ESceneGroup group, ESceneTransitionPhase phase, string error = null)
        {
            EventBus<SceneTransitionEvent>.Raise(new SceneTransitionEvent(group, phase, error));
        }

        #endregion

#if UNITY_EDITOR
        [Button]
        private void ClearCache()
        {
            Caching.ClearCache();
        }
#endif
    }
}
