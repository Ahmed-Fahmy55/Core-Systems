using Sirenix.OdinInspector;
using System;
using System.Data.SqlTypes;
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
    public class AssetVariableRef<T> : INullable where T : UnityEngine.Object
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
        /// Returns the currently referenced asset (either direct or loaded addressable).
        /// </summary>
        public T Asset => Source switch
        {
            AssetSource.Direct => _directAsset,
            AssetSource.Addressable => _loadedAsset,
            _ => null
        };

        public bool IsNull => Asset == null;


        /// <summary>
        /// Asynchronously loads the addressable asset if necessary.
        /// If already loaded, returns a completed handle with the existing asset.
        /// </summary>
        public AsyncOperationHandle<T> LoadAssetAsync()
        {
            switch (Source)
            {
                case AssetSource.Direct:
                    return CreateCompletedHandle(_directAsset);

                case AssetSource.Addressable:
                    if (_addressableAsset == null)
                        throw new InvalidOperationException("Addressable asset reference is null.");

                    // If already loaded, reuse the existing handle
                    if (_addressableAsset.OperationHandle.IsValid())
                    {
                        var existingHandle = _addressableAsset.OperationHandle.Convert<T>();
                        _loadedAsset = existingHandle.Result;
                        _handle = existingHandle;
                        return existingHandle;
                    }

                    // Otherwise, load and cache
                    _handle = _addressableAsset.LoadAssetAsync();
                    _handle.Value.Completed += OnAssetLoaded;

                    return _handle.Value;

                default:
                    throw new NotSupportedException($"Unsupported asset source: {Source}");
            }
        }


        /// <summary>
        /// Releases the loaded asset (if Addressable). Safe to call multiple times.
        /// </summary>
        public void ReleaseAsset()
        {
            if (Source == AssetSource.Addressable && _addressableAsset != null && _addressableAsset.Asset != null)
            {
                _addressableAsset.ReleaseAsset();
                _loadedAsset = null;
            }

            if (_handle.HasValue && _handle.Value.IsValid())
            {
                Addressables.Release(_handle.Value);
                _handle = null;
            }
        }

        private void OnAssetLoaded(AsyncOperationHandle<T> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                _loadedAsset = handle.Result;
        }

        /// <summary>
        /// Helper to create a completed handle for direct assets.
        /// </summary>
        private AsyncOperationHandle<T> CreateCompletedHandle(T asset)
        {
            var handle = Addressables.ResourceManager.CreateCompletedOperation(asset, string.Empty);
            return handle;
        }
    }
}
