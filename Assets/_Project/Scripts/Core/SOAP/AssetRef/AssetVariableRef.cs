using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Zone8.SOAP.AssetVariable
{
    public enum AssetSource
    {
        Direct,
        Addressable
    }

    [Serializable]
    public class AssetVariableRef<T> where T : UnityEngine.Object
    {
        [SerializeField, HideLabel, EnumToggleButtons]
        public AssetSource Source;

        [ShowIf("@Source == AssetSource.Direct")]
        [HideLabel, SerializeField]
        private T _directAsset;

        [ShowIf("@Source == AssetSource.Addressable")]
        [HideLabel, SerializeField]
        private AssetReferenceT<T> _addressableAsset;

        private T _loadedAsset;
        private AsyncOperationHandle<T>? _handle;

        /// <summary>
        /// Returns the currently referenced asset. 
        /// For Addressables, this returns null until the load operation completes successfully.
        /// </summary>
        public T Asset => Source switch
        {
            AssetSource.Direct => _directAsset,
            AssetSource.Addressable => _loadedAsset,
            _ => null
        };

        public bool HasValue => Asset != null;

        /// <summary>
        /// Asynchronously loads the asset. If already loading or loaded, returns the existing handle.
        /// </summary>
        public AsyncOperationHandle<T> LoadAssetAsync()
        {
            if (Source == AssetSource.Direct)
            {
                return Addressables.ResourceManager.CreateCompletedOperation(_directAsset, string.Empty);
            }

            if (_handle.HasValue && _handle.Value.IsValid())
            {
                // Safety check: if the operation finished while we weren't looking, sync the result
                if (_handle.Value.IsDone && _loadedAsset == null && _handle.Value.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedAsset = _handle.Value.Result;
                }

                return _handle.Value;
            }

            if (_addressableAsset == null || string.IsNullOrEmpty(_addressableAsset.AssetGUID))
            {
                Logger.LogError($"[AssetVariableRef] Addressable reference for {typeof(T).Name} is null or invalid.");
                return Addressables.ResourceManager.CreateCompletedOperation<T>(null, "Invalid Addressable Reference");
            }

            var newHandle = _addressableAsset.LoadAssetAsync();
            _handle = newHandle;

            newHandle.Completed += h =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    _loadedAsset = h.Result;
                }
                else
                {
                    Logger.LogError($"[AssetVariableRef] Failed to load asset of type {typeof(T).Name}. Status: {h.Status}");
                    // Clear handle on failure so a retry can be attempted
                    _handle = null;
                }
            };

            return newHandle;
        }

        /// <summary>
        /// Releases the loaded asset and resets the internal state. Safe to call multiple times.
        /// </summary>
        public void ReleaseAsset()
        {
            _loadedAsset = null;

            if (_handle.HasValue && _handle.Value.IsValid())
            {
                Addressables.Release(_handle.Value);
                _handle = null;
            }
        }
    }
}