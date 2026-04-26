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
    public class SceneDownloadHandler
    {
        #region Members

        private float _progressCheckInterval = 1f;
        private int _maxRetries = 2;
        private float _maxIdleTimeInSeconds = 30f;

        // CRITICAL: Tracks handles to keep assets in RAM while playing.
        // This prevents the "climbing memory" by preventing redundant re-loads.
        private Dictionary<string, AsyncOperationHandle> _managedHandles = new();

        #endregion

        #region Constructors

        public SceneDownloadHandler(float progressCheckInterval, int maxRetries, float maxIdleTime)
        {
            _progressCheckInterval = progressCheckInterval;
            _maxRetries = maxRetries;
            _maxIdleTimeInSeconds = maxIdleTime;
        }

        public SceneDownloadHandler()
        {
            _progressCheckInterval = 1;
            _maxRetries = 2;
            _maxIdleTimeInSeconds = 30;
        }

        #endregion

        #region Public API

        public void SetRetryPolicy(int maxRetries, float timeout)
        {
            _maxRetries = maxRetries;
            _maxIdleTimeInSeconds = timeout;
        }

        /// <summary>
        /// Call this when returning to the Main Menu or changing SceneGroups 
        /// to finally release memory.
        /// </summary>
        public void ReleaseHandles()
        {
            foreach (var handle in _managedHandles.Values)
            {
                if (handle.IsValid()) Addressables.Release(handle);
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

        public async Awaitable<bool> DownloadsSceneGroupDependencies(SceneGroup group, IAddressableProgressor progressor = null)
        {
            foreach (var sceneData in group.Scenes)
            {
                if (!sceneData.IsAddressable) continue;

                EventBus<BundleDownloadingEvent>.Raise(new()
                {
                    Description = $"Preparing: {group.GroupName.DisplayName}",
                    Progressor = progressor,
                    State = EDwonloadingState.Preparing,
                });

                // Optimization: Don't check size if we already hold the handle
                if (_managedHandles.ContainsKey(sceneData.Scene.Address)) continue;

                AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(sceneData.Scene.Address);
                await sizeHandle.Task;

                try
                {
                    if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (sizeHandle.Result > 0)
                        {
                            progressor?.Init(sizeHandle.Result / (1024 * 1024));

                            EventBus<BundleDownloadingEvent>.Raise(new()
                            {
                                Description = $"Downloading: {group.GroupName.DisplayName}",
                                Progressor = progressor,
                                State = EDwonloadingState.Downloading,
                            });

                            bool success = await DownloadDependenciesWithDynamicTimeoutAsync(sceneData.Scene.Address, progressor);
                            if (!success) return false;
                        }
                    }
                    else return false;
                }
                finally
                {
                    if (sizeHandle.IsValid()) Addressables.Release(sizeHandle);
                }
            }
            return true;
        }

        public async Awaitable<bool> DownloadBundle(string label, IAddressableProgressor progressor = null)
        {
            if (_managedHandles.ContainsKey(label)) return true;

            AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(label);
            await sizeHandle.Task;

            try
            {
                if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (sizeHandle.Result > 0)
                    {
                        progressor?.Init(sizeHandle.Result / (1024 * 1024));

                        EventBus<BundleDownloadingEvent>.Raise(new()
                        {
                            Description = $"Downloading: {label}",
                            Progressor = progressor,
                            State = EDwonloadingState.Downloading,
                        });
                        return await DownloadDependenciesWithDynamicTimeoutAsync(label, progressor);
                    }
                    return true;
                }
                return false;
            }
            finally
            {
                if (sizeHandle.IsValid()) Addressables.Release(sizeHandle);
            }
        }

        #endregion

        #region Private Methods

        private async Awaitable<bool> DownloadDependenciesWithDynamicTimeoutAsync(string address, IAddressableProgressor progressor = null)
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
                            if (idleTime >= _maxIdleTimeInSeconds) break;
                        }

                        progressor?.Progress(currentProgress);
                        await Awaitable.WaitForSecondsAsync(_progressCheckInterval);
                    }

                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        isDownloaded = true;
                        // BUG FIX: Store the handle to maintain ref-count
                        if (_managedHandles.ContainsKey(address)) Addressables.Release(_managedHandles[address]);
                        _managedHandles[address] = handle;

                        EventBus<BundleDownloadingEvent>.Raise(new BundleDownloadingEvent()
                        {
                            Description = $"Success donwloading bundle: {address}",
                            State = EDwonloadingState.Finished,
                            Progressor = progressor,
                        });
                    }
                    else
                    {
                        // Clean up failed handle so we can retry
                        Addressables.Release(handle);

                        EventBus<BundleDownloadingEvent>.Raise(new BundleDownloadingEvent()
                        {
                            Description = $"Failed downloading bundle: {address}",
                            Progressor = progressor,
                            State = EDwonloadingState.Failiure,
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[SceneDownloadHandler] {ex.Message}");
                    if (handle.IsValid()) Addressables.Release(handle);
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