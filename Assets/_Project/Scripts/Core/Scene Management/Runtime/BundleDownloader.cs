using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Zone8.SceneManagement
{
    /// <summary>Prefetches addressable bundles by label, optionally on start.</summary>
    public class BundleDownloader : MonoBehaviour
    {
        [SerializeField] private string[] _labels;
        [SerializeField] private bool _downloadOnStart;

        private ISceneManager _sceneManager;

        private void Awake()
        {
            _sceneManager = FindAnyObjectByType<SceneManagementManager>();
        }

        private void Start()
        {
            if (_downloadOnStart)
                _ = Download();
        }

        /// <summary>
        /// Downloads all configured labels. Blank entries and labels that don't exist in
        /// Addressables are skipped (a config error can never be fixed by retrying, so it
        /// must not block the caller) — an empty result is a valid no-op that succeeds
        /// right away. Only real download failures return false.
        /// </summary>
        public async Awaitable<bool> Download()
        {
            string[] labels = _labels == null
                ? System.Array.Empty<string>()
                : _labels.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();

            if (_labels != null && labels.Length != _labels.Length)
                Logger.LogWarning($"[BundleDownloader] Skipped {_labels.Length - labels.Length} blank label(s) on '{name}'.", this);

            if (labels.Length == 0)
                return true;

            if (_sceneManager == null)
            {
                Logger.LogError("[BundleDownloader] No SceneManagementManager found — cannot download bundles.", this);
                return false;
            }

            var validLabels = new List<string>(labels.Length);
            foreach (var label in labels)
            {
                if (await LabelExists(label))
                    validLabels.Add(label);
                else
                    Logger.LogError($"[BundleDownloader] Label '{label}' does not exist in Addressables — skipping it. Check the labels on '{name}'.", this);
            }

            if (validLabels.Count == 0)
                return true;

            return await _sceneManager.DownloadBundles(validLabels.ToArray());
        }

        /// <summary>
        /// True when the label resolves to at least one addressable asset. An unknown key
        /// completes successfully with an empty location list, so this never throws.
        /// </summary>
        private static async Awaitable<bool> LabelExists(string label)
        {
            AsyncOperationHandle<IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> handle =
                Addressables.LoadResourceLocationsAsync(label);
            await handle.Task;

            bool exists = handle.Status == AsyncOperationStatus.Succeeded && handle.Result is { Count: > 0 };
            Addressables.Release(handle);
            return exists;
        }
    }
}
