using Sirenix.OdinInspector;
using UnityEngine;
using Zone8.Fading;

namespace Zone8.SceneManagement
{

    public class SceneManagementManager : SceneManagementBase
    {
        [SerializeField] private ESceneGroup defaultScene;
        [SerializeField] private bool loadDefaultSceneOnStart;

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
            await _fader.FadeIn();
        }

        public override async Awaitable EndLoadingEffect()
        {
            await _fader.FadeOut();
        }

        public override void Report(float loadingProgress)
        {
        }
    }
}