using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Zone8.Events;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// Downloads addressable dependencies (scene-group scenes and standalone bundles) with a
    /// stall-detecting retry policy, and keeps the resulting handles alive so downloaded assets
    /// stay in memory instead of being redundantly re-loaded.
    /// </summary>
    public class SceneDownloadHandler
    {
        #region Members

        private readonly float _progressCheckInterval;
        private readonly int _maxRetries;
        private readonly float _maxIdleTimeInSeconds;

        // CRITICAL: Tracks handles to keep assets in RAM while playing.
        // This prevents the "climbing memory" by preventing redundant re-loads.
        private readonly Dictionary<string, AsyncOperationHandle> _managedHandles = new();

        #endregion

        public SceneDownloadHandler(float progressCheckInterval = 1f, int maxRetries = 2, float maxIdleTime = 30f)
        {
            _progressCheckInterval = progressCheckInterval;
            _maxRetries = maxRetries;
            _maxIdleTimeInSeconds = maxIdleTime;
        }

        #region Public API

        /// <summary>
        /// Call this when returning to the Main Menu or changing SceneGroups
        /// to finally release memory.
        /// </summary>
        public void ReleaseHandles()
        {
            foreach (var handle in _managedHandles.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            _managedHandles.Clear();
        }

        public void ReleaseHandle(string address)
        {
            if (_managedHandles.TryGetValue(address, out var handle))
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
                _managedHandles.Remove(address);
            }
            else
            {
                Logger.LogWarning($"[SceneDownloadHandler] No managed handle found for: {address}");
            }
        }

        /// <summary>Downloads the dependencies of every addressable scene in the group.</summary>
        public async Awaitable<bool> DownloadSceneGroupDependencies(SceneGroup group, IAddressableProgressor progressor = null)
        {
            foreach (var sceneData in group.Scenes)
            {
                if (!sceneData.IsAddressable)
                    continue;

                if (!await EnsureDownloadedAsync(sceneData.Scene.Address, group.GroupName, progressor))
                    return false;
            }
            return true;
        }

        /// <summary>Downloads a standalone bundle by label.</summary>
        public Awaitable<bool> DownloadBundle(string label, IAddressableProgressor progressor = null)
            => EnsureDownloadedAsync(label, null, progressor);

        #endregion

        #region Private Methods

        /// <summary>
        /// Shared download flow for scene dependencies and standalone bundles:
        /// skip if already held, check remote size, then download with retry.
        /// </summary>
        private async Awaitable<bool> EnsureDownloadedAsync(string address, ESceneGroup owningGroup, IAddressableProgressor progressor)
        {
            if (_managedHandles.ContainsKey(address))
            {
                EventBus<BundleDownloadEvent>.Raise(BundleDownloadEvent.Completed(owningGroup, address));
                return true;
            }

            EventBus<BundleDownloadEvent>.Raise(BundleDownloadEvent.Preparing(owningGroup, address));

            AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(address);
            await sizeHandle.Task;

            try
            {
                if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    EventBus<BundleDownloadEvent>.Raise(
                        BundleDownloadEvent.Failed(owningGroup, address, sizeHandle.Status.ToString()));
                    return false;
                }

                if (sizeHandle.Result <= 0)
                {
                    // nothing to download: already cached locally
                    EventBus<BundleDownloadEvent>.Raise(BundleDownloadEvent.Completed(owningGroup, address));
                    return true;
                }

                progressor?.Init(sizeHandle.Result / (1024 * 1024));
                EventBus<BundleDownloadEvent>.Raise(
                    BundleDownloadEvent.Downloading(owningGroup, address, sizeHandle.Result));

                return await DownloadWithRetryAsync(address, owningGroup, progressor);
            }
            finally
            {
                if (sizeHandle.IsValid())
                    Addressables.Release(sizeHandle);
            }
        }

        /// <summary>
        /// Downloads dependencies for an address, retrying when the download fails or stalls
        /// (no progress for <see cref="_maxIdleTimeInSeconds"/>).
        /// </summary>
        private async Awaitable<bool> DownloadWithRetryAsync(string address, ESceneGroup owningGroup, IAddressableProgressor progressor)
        {
            int attempt = 0;
            bool isDownloaded = false;

            while (attempt < _maxRetries && !isDownloaded)
            {
                attempt++;
                var handle = Addressables.DownloadDependenciesAsync(address);

                float lastProgress = 0f;
                float idleTime = 0f;

                try
                {
                    while (!handle.IsDone)
                    {
                        float currentProgress = handle.PercentComplete;
                        if (Math.Abs(currentProgress - lastProgress) > 0.001f)
                        {
                            lastProgress = currentProgress;
                            idleTime = 0f;
                        }
                        else
                        {
                            idleTime += _progressCheckInterval;
                            if (idleTime >= _maxIdleTimeInSeconds)
                                break;
                        }

                        progressor?.Progress(currentProgress);
                        await Awaitable.WaitForSecondsAsync(_progressCheckInterval);
                    }

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        isDownloaded = true;
                        // Store the handle to maintain ref-count and prevent redundant re-loads
                        if (_managedHandles.TryGetValue(address, out var previous))
                            Addressables.Release(previous);
                        _managedHandles[address] = handle;

                        EventBus<BundleDownloadEvent>.Raise(BundleDownloadEvent.Completed(owningGroup, address));
                    }
                    else
                    {
                        // Clean up failed handle so we can retry
                        Addressables.Release(handle);

                        EventBus<BundleDownloadEvent>.Raise(
                            BundleDownloadEvent.Failed(owningGroup, address, $"Download failed (attempt {attempt}/{_maxRetries})."));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[SceneDownloadHandler] {ex.Message}");
                    if (handle.IsValid())
                        Addressables.Release(handle);
                }
            }
            return isDownloaded;
        }

        #endregion
    }

    public readonly struct AsyncOperationHandleGroup
    {
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);

        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }
}
