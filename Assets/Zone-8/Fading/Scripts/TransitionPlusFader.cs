using Sirenix.OdinInspector;
using System;
using TransitionsPlus;
using UnityEngine;

namespace Zone8.Fading
{
    public class TransitionPlusFader : MonoBehaviour, IFader
    {
        [SerializeField] private TransitionAnimator _transitionAnimator;
        [SerializeField] private bool _hideOnFadeOut;

        private void Awake()
        {
            if (_transitionAnimator) _transitionAnimator.gameObject.SetActive(false);
        }

        [Button]
        public void FadeIn(float duration = 0, Action onComplete = null)
        {
            Fade(false, duration, onComplete);
        }

        [Button]
        public void FadeOut(float duration = 0, Action onComplete = null)
        {
            Fade(true, duration, onComplete);
        }

        private void Fade(bool invert, float duration = 0, Action onComplete = null)
        {
            if (_transitionAnimator == null)
            {
                Debug.LogError("TransitionAnimator is not assigned.");
                return;
            }
            _transitionAnimator.gameObject.SetActive(true);

            if (duration > 0)
            {
                _transitionAnimator.profile.duration = duration;
            }
            _transitionAnimator.profile.invert = invert;

            _transitionAnimator.onTransitionEnd.RemoveAllListeners();
            _transitionAnimator.onTransitionEnd.AddListener(() =>
            {
                onComplete?.Invoke();
                if (invert) _transitionAnimator.gameObject.SetActive(false);
                if (!invert && _hideOnFadeOut) _transitionAnimator.gameObject.SetActive(false);
            });

            _transitionAnimator.Play();

        }
    }
}
