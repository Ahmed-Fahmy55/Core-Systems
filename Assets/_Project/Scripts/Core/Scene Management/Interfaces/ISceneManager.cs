using System;
using UnityEngine;

namespace Zone8.SceneManagement
{
    public interface ISceneManager
    {
        Awaitable Load(ESceneGroup groupName, string[] relatedBundles = null,
            IProgress<float> loadProgressor = null, IAddressableProgressor downloadingProgressor = null);

        Awaitable<bool> DownloadBundles(string[] relatedBundles, IAddressableProgressor progressor);

        Awaitable ReloadCurrentSceneGroup();

        void RegisterSceneGroup(SceneGroup group);
        void UnregisterSceneGroup(SceneGroup group);

        void ClearHandles();
        void ClearHandle(string label);
    }
}
