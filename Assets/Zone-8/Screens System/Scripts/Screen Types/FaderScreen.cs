using Zone8.Fading;
using UnityEngine;

namespace Zone8.Screens
{
    public class FadeScreen : ScreenBase
    {

        [SerializeField] private IFader _fader;
        [SerializeField] private float _fadeTime;


        public override async Awaitable StartHideEffect()
        {
            if (_fader == null) return;

            _fader.FadeIn(_fadeTime);
            await Awaitable.WaitForSecondsAsync(_fadeTime);
        }


        public override async Awaitable StartShowEffect()
        {
            if (_fader == null) return;

            _fader.FadeOut(_fadeTime);
            await Awaitable.WaitForSecondsAsync(_fadeTime);
        }
    }
}
