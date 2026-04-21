using Zone8.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// Event raised during the scene downloading process, providing information about the progress and state.
    /// </summary>
    public struct SceneDownloadingEvent : IEvent
    {
        /// <summary>
        /// Progress reporter for the downloading process.
        /// </summary>
        public IAddressableProgressor Progressor;

        /// <summary>
        /// Description of the current downloading state.
        /// </summary>
        public string Description;

        /// <summary>
        /// Current state of the downloading process.
        /// </summary>
        public EDwonloadingState State;
    }

    /// <summary>
    /// Enum representing the various states of the downloading process.
    /// </summary>
    public enum EDwonloadingState
    {
        None,
        Preparing,
        Downloading,
        Finished,
        Failiure
    }

    /// <summary>
    /// Handles the downloading of scene dependencies and bundles using Unity's Addressables system.
    /// </summary>
    public class SceneDownloadHandler
    {
        #region Members

        /// <summary>
        /// Interval (in seconds) to check the progress of downloads.
        /// </summary>
        private float _progressCheckInterval;

        /// <summary>
        /// Maximum number of retry attempts for failed downloads.
        /// </summary>
        private int _maxRetries;

        /// <summary>
        /// Maximum idle time (in seconds) before a download is considered failed.
        /// </summary>
        private float _maxIdleTimeInSeconds;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneDownloadHandler"/> class with custom settings.
        /// </summary>
        /// <param name="progressCheckInterval">Interval (in seconds) to check download progress.</param>
        /// <param name="maxRetries">Maximum number of retry attempts for failed downloads.</param>
        /// <param name="maxIdleTime">Maximum idle time (in seconds) before a download is considered failed.</param>
        public SceneDownloadHandler(float progressCheckInterval, int maxRetries, float maxIdleTime)
        {
            _progressCheckInterval = progressCheckInterval;
            _maxRetries = maxRetries;
            _maxIdleTimeInSeconds = maxIdleTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneDownloadHandler"/> class with default settings.
        /// </summary>
        public SceneDownloadHandler()
        {
            _progressCheckInterval = 1;
            _maxRetries = 2;
            _maxIdleTimeInSeconds = 30;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the retry policy for failed downloads.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <param name="timeout">Maximum idle time (in seconds) before a download is considered failed.</param>
        public void SetRetryPolicy(int maxRetries, float timeout)
        {
            _maxRetries = maxRetries;
            _maxIdleTimeInSeconds = timeout;
        }

        /// <summary>
        /// Downloads Addressable dependencies for all scenes in the provided <see cref="SceneGroup"/>.
        /// </summary>
        /// <param name="group">The <see cref="SceneGroup"/> containing scenes to download dependencies for.</param>
        /// <param name="progressor">Optional progress reporter for tracking the download process.</param>
        /// <returns>A task that resolves to <c>true</c> if all dependencies are downloaded successfully; otherwise, <c>false</c>.</returns>
        public async Awaitable<bool> DownloadsSceneGroupDependencies(SceneGroup group, IAddressableProgressor progressor = null)
        {
            foreach (var sceneData in group.Scenes)
            {
                if (!sceneData.IsAddressable) continue;

                EventBus<SceneDownloadingEvent>.Raise(new()
                {
                    Description = $"Checking Resources for: {group.GroupName.DisplayName}",
                    Progressor = progressor,
                    State = EDwonloadingState.Preparing,
                });

                AsyncOperationHandle<long> downloadSizeHandle = default;

                try
                {
                    // Check if dependencies are already downloaded
                    downloadSizeHandle = Addressables.GetDownloadSizeAsync(sceneData.Scene.Address);
                    await downloadSizeHandle.Task;

                    long downloadSizeMB = downloadSizeHandle.Result / (1024 * 1024);

                    if (downloadSizeHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (downloadSizeHandle.Result > 0)
                        {
                            EventBus<SceneDownloadingEvent>.Raise(new()
                            {
                                Description = $"Downloading Resources for: {group.GroupName.DisplayName}",
                                Progressor = progressor,
                                State = EDwonloadingState.Downloading,
                            });

                            bool downloadSuccess = await DownloadDependenciesWithDynamicTimeoutAsync(sceneData.Scene.Address, downloadSizeMB, progressor);

                            if (!downloadSuccess)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return false;
                }
                finally
                {
                    if (downloadSizeHandle.IsValid())
                        downloadSizeHandle.Release();
                }
            }
            return true;
        }

        /// <summary>
        /// Downloads dependencies for a specific bundle identified by its label.
        /// </summary>
        /// <param name="label">The label used to identify Addressable assets.</param>
        /// <param name="progressor">Optional progress reporter for tracking the download process.</param>
        /// <returns>A task that resolves to <c>true</c> if the bundle is downloaded successfully; otherwise, <c>false</c>.</returns>
        public async Awaitable<bool> DownloadBundle(string label, IAddressableProgressor progressor = null)
        {
            EventBus<SceneDownloadingEvent>.Raise(new()
            {
                Description = $"Checking dependencies for {label} Bundle",
                Progressor = progressor,
                State = EDwonloadingState.Preparing,
            });

            AsyncOperationHandle<long> downloadSizeHandle = default;

            try
            {
                downloadSizeHandle = Addressables.GetDownloadSizeAsync(label);
                await downloadSizeHandle.Task;

                if (downloadSizeHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    long downloadSize = downloadSizeHandle.Result / (1024 * 1024);

                    if (downloadSize > 0)
                    {
                        EventBus<SceneDownloadingEvent>.Raise(new()
                        {
                            Description = $"Downloading {label} Bundle",
                            Progressor = progressor,
                            State = EDwonloadingState.Downloading,
                        });

                        bool downloadSuccess = await DownloadDependenciesWithDynamicTimeoutAsync(label, downloadSize, progressor);

                        if (!downloadSuccess)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            finally
            {
                if (downloadSizeHandle.IsValid())
                    downloadSizeHandle.Release();
            }
            return true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Downloads dependencies with dynamic timeout handling.
        /// </summary>
        /// <param name="address">The address of the dependencies to download.</param>
        /// <param name="downloadSizeMB">The size of the download in megabytes.</param>
        /// <param name="progressor">Optional progress reporter for tracking the download process.</param>
        /// <returns>A task that resolves to <c>true</c> if the dependencies are downloaded successfully; otherwise, <c>false</c>.</returns>
        private async Awaitable<bool> DownloadDependenciesWithDynamicTimeoutAsync(string address, long downloadSizeMB, IAddressableProgressor progressor = null)
        {
            int attempt = 0;
            bool isDownloaded = false;

            while (attempt < _maxRetries && !isDownloaded)
            {
                attempt++;
                var dependencyHandle = Addressables.DownloadDependenciesAsync(address);
                progressor?.Init(downloadSizeMB);

                float lastProgress = 0f;
                float idleTime = 0f;

                try
                {
                    while (!dependencyHandle.IsDone)
                    {
                        float currentProgress = dependencyHandle.PercentComplete;

                        if (Math.Abs(currentProgress - lastProgress) > 0.001f)
                        {
                            lastProgress = currentProgress;
                            idleTime = 0f;
                        }
                        else
                        {
                            idleTime += _progressCheckInterval;
                            if (idleTime >= _maxIdleTimeInSeconds)
                            {
                                break;
                            }
                        }

                        progressor?.Report(currentProgress);
                        await Awaitable.WaitForSecondsAsync(_progressCheckInterval);
                    }

                    if (dependencyHandle.IsDone && dependencyHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        isDownloaded = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error during download operation: {ex.Message}");
                }
                finally
                {
                    if (dependencyHandle.IsValid()) dependencyHandle.Release();
                }
            }
            return isDownloaded;
        }

        #endregion
    }

    /// <summary>
    /// Represents a group of Addressable async operation handles.
    /// </summary>
    public readonly struct AsyncOperationHandleGroup
    {
        /// <summary>
        /// List of async operation handles in the group.
        /// </summary>
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        /// <summary>
        /// Overall progress of the group.
        /// </summary>
        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);

        /// <summary>
        /// Indicates whether all operations in the group are complete.
        /// </summary>
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncOperationHandleGroup"/> struct.
        /// </summary>
        /// <param name="initialCapacity">Initial capacity of the handles list.</param>
        public AsyncOperationHandleGroup(int initialCapacity)
        {
            Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);
        }
    }

    /// <summary>
    /// Interface for reporting progress of Addressable downloads.
    /// </summary>
    public interface IAddressableProgressor
    {
        /// <summary>
        /// Initializes the progress reporter with the total download size.
        /// </summary>
        /// <param name="downloadSize">Total download size in megabytes.</param>
        void Init(long downloadSize);

        /// <summary>
        /// Reports the current progress of the download.
        /// </summary>
        /// <param name="progress">Current progress as a value between 0 and 1.</param>
        void Report(float progress);
    }
}
