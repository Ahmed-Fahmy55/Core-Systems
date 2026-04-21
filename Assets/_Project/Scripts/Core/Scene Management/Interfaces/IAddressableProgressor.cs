namespace Zone8.SceneManagement
{
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
        /// 
        void Progress(float progress);
    }
}
