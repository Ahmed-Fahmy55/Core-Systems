using Sirenix.OdinInspector;
using UnityEngine;
using Zone8.Fading;

namespace Zone8.SceneManagement
{

    public class SceneManagementManager : SceneManagementBase
    {
        [SerializeField] private ESceneGroup defaultScene;
        [SerializeField] private bool loadDefaultSceneOnStart;
        [SerializeField] private float _fadInDuration = 1;
        [SerializeField] private float _fadeOutDuration = 1;

        private IFader _fader;

        protected override void Awake()
        {
            base.Awake();
            _fader = GetComponentInChildren<IFader>();
            if (_fader == null)
            {
                Logger.LogError("No IFader component found in children of SceneManagementManager. Please add one to enable fading effects.");
            }
        }

        private void Start()
        {
            if (loadDefaultSceneOnStart) LoadDefaultScene();
        }


        [Button]
        public void LoadDefaultScene()
        {
            Load(defaultScene);
        }

        public override async Awaitable StartLoadingEffect()
        {
            _fader.FadeIn(_fadInDuration);
            await Awaitable.WaitForSecondsAsync(_fadInDuration);
        }

        public override async Awaitable EndLoadingEffect()
        {
            _fader.FadeOut(_fadeOutDuration);
            await Awaitable.WaitForSecondsAsync(_fadeOutDuration);
        }

        public override void Report(float loadingProgress)
        {
        }
    }
}