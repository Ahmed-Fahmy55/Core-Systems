using Michsky.UI.Heat;
using UnityEngine;
using Zone8.Events;

namespace Zone8.SceneManagement
{
    public class DownloadProgressorUI : MonoBehaviour, IAddressableProgressor
    {
        [SerializeField] private ProgressBar _progressBar;

        private EventBinding<BundleDownloadEvent> _bundleDownloadBinding;
        private long _currentDownloadSize;

        private void Awake()
        {
            _bundleDownloadBinding = new EventBinding<BundleDownloadEvent>(OnBundleDownload);
            EventBus<BundleDownloadEvent>.Register(_bundleDownloadBinding);
        }

        private void OnDestroy()
        {
            EventBus<BundleDownloadEvent>.Deregister(_bundleDownloadBinding);
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

        private void OnBundleDownload(BundleDownloadEvent data)
        {
            // The bar value itself is driven directly through IAddressableProgressor.Progress;
            // this handler only reacts to discrete phase changes for show/hide.
            switch (data.Phase)
            {
                case EDownloadPhase.Downloading:
                    _progressBar.SetValue(0);
                    gameObject.SetActive(true);
                    break;

                case EDownloadPhase.Completed:
                    _progressBar.SetValue(1);
                    gameObject.SetActive(false);
                    break;

                case EDownloadPhase.Failed:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }
}
