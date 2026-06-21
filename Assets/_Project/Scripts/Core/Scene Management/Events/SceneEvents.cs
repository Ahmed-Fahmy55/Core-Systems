using Zone8.Events;

namespace Zone8.SceneManagement
{
    // ─────────────────────────────────────────────────────────────────────────
    //  Scene Management messaging cycle
    //
    //  Two channels, kept deliberately separate:
    //
    //  • Continuous progress (0..1, per-frame / per-tick) flows through the
    //    injected IProgress<float> and IAddressableProgressor side-channels.
    //    It is high frequency, so it never touches the event bus.
    //
    //  • Discrete phase notifications flow through the EventBus as the immutable,
    //    structured events below. They carry domain data only — listeners format
    //    their own display strings. No event holds a progressor reference.
    //
    //  Layers, outer → inner:
    //    SceneTransitionEvent   one per Load() call — the whole lifecycle
    //      └ BundleDownloadEvent   addressable dependency / bundle downloads
    //      └ SceneGroupLoadEvent   the group being unloaded then loaded
    //          └ SceneLoadEvent    each individual scene in the group
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Lifecycle phase of a complete scene-group transition.</summary>
    public enum ESceneTransitionPhase
    {
        Started,
        Downloading,
        Unloading,
        Loading,
        Completed,
        Failed
    }

    /// <summary>Status of a group- or scene-level load/unload operation.</summary>
    public enum ESceneLoadStatus
    {
        Loading,
        Unloading,
        Completed,
        Failed
    }

    /// <summary>Phase of an Addressable bundle / dependency download.</summary>
    public enum EDownloadPhase
    {
        Preparing,
        Downloading,
        Completed,
        Failed
    }

    /// <summary>
    /// Raised at the boundaries of a full scene-group transition
    /// (request → download → unload → load → done). This is the single event
    /// most gameplay systems should observe: loading screens, input lockout,
    /// analytics, music switching, etc.
    /// </summary>
    public readonly struct SceneTransitionEvent : IEvent
    {
        public readonly ESceneGroup Group;
        public readonly ESceneTransitionPhase Phase;
        /// <summary>Populated only when <see cref="Phase"/> is <see cref="ESceneTransitionPhase.Failed"/>.</summary>
        public readonly string Error;

        public SceneTransitionEvent(ESceneGroup group, ESceneTransitionPhase phase, string error = null)
        {
            Group = group;
            Phase = phase;
            Error = error;
        }
    }

    /// <summary>Raised when a scene group starts/finishes loading or unloading.</summary>
    public readonly struct SceneGroupLoadEvent : IEvent
    {
        public readonly SceneGroup SceneGroup;
        public readonly ESceneLoadStatus Status;

        public SceneGroupLoadEvent(SceneGroup sceneGroup, ESceneLoadStatus status)
        {
            SceneGroup = sceneGroup;
            Status = status;
        }
    }

    /// <summary>Raised for each individual scene as it loads or unloads.</summary>
    public readonly struct SceneLoadEvent : IEvent
    {
        public readonly SceneData SceneData;
        public readonly ESceneLoadStatus Status;

        public SceneLoadEvent(SceneData sceneData, ESceneLoadStatus status)
        {
            SceneData = sceneData;
            Status = status;
        }
    }

    /// <summary>
    /// Raised as Addressable dependencies / bundles are downloaded. Carries
    /// structured data only — continuous percentage flows through the
    /// <see cref="IAddressableProgressor"/>; this event marks phase changes.
    /// </summary>
    public readonly struct BundleDownloadEvent : IEvent
    {
        /// <summary>The owning scene group, or <c>null</c> for a standalone bundle download.</summary>
        public readonly ESceneGroup Group;
        /// <summary>The bundle label or scene address being downloaded.</summary>
        public readonly string Label;
        public readonly EDownloadPhase Phase;
        /// <summary>Total payload size in bytes. Known only from <see cref="EDownloadPhase.Downloading"/> onward.</summary>
        public readonly long TotalBytes;
        /// <summary>Populated only when <see cref="Phase"/> is <see cref="EDownloadPhase.Failed"/>.</summary>
        public readonly string Error;

        BundleDownloadEvent(ESceneGroup group, string label, EDownloadPhase phase, long totalBytes, string error)
        {
            Group = group;
            Label = label;
            Phase = phase;
            TotalBytes = totalBytes;
            Error = error;
        }

        public static BundleDownloadEvent Preparing(ESceneGroup group, string label) =>
            new(group, label, EDownloadPhase.Preparing, 0, null);

        public static BundleDownloadEvent Downloading(ESceneGroup group, string label, long totalBytes) =>
            new(group, label, EDownloadPhase.Downloading, totalBytes, null);

        public static BundleDownloadEvent Completed(ESceneGroup group, string label) =>
            new(group, label, EDownloadPhase.Completed, 0, null);

        public static BundleDownloadEvent Failed(ESceneGroup group, string label, string error) =>
            new(group, label, EDownloadPhase.Failed, 0, error);
    }
}
