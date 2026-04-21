using Michsky.UI.Heat;
using UnityEngine;
using Zone8.Events;

namespace Zone8.SceneManagement
{
    public class DownloadProgressorUI : MonoBehaviour, IAddressableProgressor
    {
        [SerializeField] private ProgressBar _progressBar;

        private EventBinding<BundleDownloadingEvent> _bundleDownloadBinding;
        private long _currentDownloadSize;

        private void Awake()
        {
            _bundleDownloadBinding = new EventBinding<BundleDownloadingEvent>(OnBundleDownloading);
            EventBus<BundleDownloadingEvent>.Register(_bundleDownloadBinding);
        }

        private void OnDestroy()
        {
            EventBus<BundleDownloadingEvent>.Deregister(_bundleDownloadBinding);
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void Init(long downloadSize)
        {
            _currentDownloadSize = downloadSize;
        }

        public void Progress(float progress)
        {
            _progressBar.SetValue(progress);
            if (_currentDownloadSize > 0)
            {
                //Update text or somthing
            }
        }

        private void OnBundleDownloading(BundleDownloadingEvent data)
        {
            if (data.Progressor != (IAddressableProgressor)this) return;

            switch (data.State)
            {
                case EDwonloadingState.Downloading:
                    _progressBar.SetValue(0);
                    gameObject.SetActive(true);
                    break;

                case EDwonloadingState.Finished:
                    _progressBar.SetValue(1);
                    gameObject.SetActive(false);
                    break;

                case EDwonloadingState.Failiure:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }
}
