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

        /// <summary>
        /// Get the asset synchronously (works only for direct asset or already-loaded addressable).
        /// </summary>
        public T Asset
        {
            get
            {
                return Source switch
                {
                    AssetSource.Direct => _directAsset,
                    AssetSource.Addressable => _loadedAsset,
                    _ => null
                };
            }
        }

        public bool IsNull => Source switch
        {
            AssetSource.Direct => _directAsset == null,
            AssetSource.Addressable => _addressableAsset == null,
            _ => true
        };

        /// <summary>
        /// Asynchronously load the addressable asset (if in Addressable mode).
        /// </summary>
        public AsyncOperationHandle<T> LoadAssetAsync()
        {
            if (Source == AssetSource.Addressable && _addressableAsset != null)
            {
                var handle = _addressableAsset.LoadAssetAsync();
                handle.Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        _loadedAsset = op.Result;
                    }
                };
                return handle;
            }

            return default;
        }

        /// <summary>
        /// Release the loaded asset when you're done using it.
        /// </summary>
        public void ReleaseAsset()
        {
            if (_loadedAsset != null && _addressableAsset != null)
            {
                _addressableAsset.ReleaseAsset();
                _loadedAsset = null;
            }
        }
    }
}
