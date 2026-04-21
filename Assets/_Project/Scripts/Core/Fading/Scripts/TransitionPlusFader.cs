using Sirenix.OdinInspector;
using System;
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
        public async Awaitable FadeIn(Action onComplete = null)
        {
            await Fade(false, onComplete);
        }

        [Button]
        public async Awaitable FadeOut(Action onComplete = null)
        {
            await Fade(true, onComplete);
        }

        private async Awaitable Fade(bool isFadingOut, Action onComplete = null)
        {
            if (_transitionAnimator == null)
            {
                Debug.LogError("TransitionAnimator is not assigned.");
                return;
            }
            _transitionAnimator.gameObject.SetActive(true);
            _transitionAnimator.profile.invert = isFadingOut;

            bool _isfinished = false;
            _transitionAnimator.onTransitionEnd.RemoveAllListeners();
            _transitionAnimator.onTransitionEnd.AddListener(() =>
            {
                _isfinished = true;
            });

            _transitionAnimator.Play();

            while (!_isfinished)
            {
                await Awaitable.NextFrameAsync();
            }
            onComplete?.Invoke();
            if (isFadingOut) _transitionAnimator.gameObject.SetActive(false);
            if (!isFadingOut && _hideOnFadeIn) _transitionAnimator.gameObject.SetActive(false);

        }
    }
}
