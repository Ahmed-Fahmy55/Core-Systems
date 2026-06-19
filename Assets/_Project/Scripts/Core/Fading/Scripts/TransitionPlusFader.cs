using Sirenix.OdinInspector;
using TransitionsPlus;
using UnityEngine;

namespace Zone8.Fading
{
    public class TransitionPlusFader : MonoBehaviour, IFader
    {
        [SerializeField] private TransitionAnimator _transitionAnimator;
        [SerializeField] private bool _hideOnFadeIn;

        private void Awake()
        {
            if (_transitionAnimator) _transitionAnimator.gameObject.SetActive(false);
        }

        [Button]
        public async Awaitable FadeIn()
        {
            await Fade(isFadingOut: false);
        }

        [Button]
        public async Awaitable FadeOut()
        {
            await Fade(isFadingOut: true);
        }

        private async Awaitable Fade(bool isFadingOut)
        {
            if (_transitionAnimator == null)
            {
                Debug.LogError("TransitionAnimator is not assigned.");
                return;
            }

            _transitionAnimator.gameObject.SetActive(true);
            _transitionAnimator.profile.invert = isFadingOut;

            bool isFinished = false;
            _transitionAnimator.onTransitionEnd.RemoveAllListeners();
            _transitionAnimator.onTransitionEnd.AddListener(() => isFinished = true);
            _transitionAnimator.Play();

            while (!isFinished)
                await Awaitable.NextFrameAsync();

            if (isFadingOut || _hideOnFadeIn)
                _transitionAnimator.gameObject.SetActive(false);
        }
    }
}
