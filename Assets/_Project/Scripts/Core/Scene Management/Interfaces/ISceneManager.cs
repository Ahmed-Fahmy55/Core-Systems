using System;
using UnityEngine;

namespace Zone8.SceneManagement
{
    /// <summary>What consumers of the scene system actually need: load groups, prefetch bundles, release memory.</summary>
    public interface ISceneManager
    {
        /// <summary>Loads a scene group. Returns false when the group is unknown, a download failed, or a load is already in flight.</summary>
        Awaitable<bool> Load(ESceneGroup groupName, string[] relatedBundles = null,
            IProgress<float> loadProgressor = null, IAddressableProgressor downloadingProgressor = null);

        /// <summary>Downloads the given addressable bundles up front.</summary>
        Awaitable<bool> DownloadBundles(string[] relatedBundles, IAddressableProgressor progressor = null);

        Awaitable<bool> ReloadCurrentSceneGroup();

        /// <summary>Releases every held bundle handle (call when returning to a hub scene to free memory).</summary>
        void ClearHandles();

        /// <summary>Releases the held handle for a single bundle label.</summary>
        void ClearHandle(string label);
    }
}
