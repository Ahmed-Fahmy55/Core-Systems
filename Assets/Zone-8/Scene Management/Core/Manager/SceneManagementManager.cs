using Zone8.Fading;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.SceneManagement
{
    /// <summary>
    /// The Facade for scene management interactions
    /// </summary>
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
            await Awaitable.WaitForSecondsAsync(_fadInDuration);
        }

        public override void Report(float loadingProgress)
        {
        }
    }
}