using System;
using Zone8.Events;

namespace Zone8.SceneManagement
{
    public struct BundleDownloadingEvent : IEvent
    {
        public IAddressableProgressor Progressor;
        public string Description;
        public EDwonloadingState State;
    }

    public enum EDwonloadingState
    {
        None,
        Preparing,
        Downloading,
        Finished,
        Failiure
    }

    // Event triggered when a scene group is loaded or unloaded
    public struct SceneGroupLoadEvent : IEvent
    {
        public IProgress<float> Progressor;
        public SceneGroup SceneGroup;
        public ESceneLoadStatus LoadStatues;
    }

    // Event triggered when an individual scene is loaded or unloaded
    public struct SceneLoadEvent : IEvent
    {
        public IProgress<float> Progressor;
        public SceneData SceneData;
        public ESceneLoadStatus LoadStatues;
    }

    public enum ESceneLoadStatus
    {
        None, Started, Loading, Unloading, Completed, Error, FinishedUnloading, FinishedLoading,
    }
}
