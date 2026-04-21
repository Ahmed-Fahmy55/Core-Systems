using System;
using UnityEngine;

namespace Zone8.Fading
{
    [RequireComponent(typeof(Animator))]
    public class AnimationAnimator : MonoBehaviour, IFader
    {

        private const string k_Open = "Open";
        private const string k_Close = "Close";

        private Animator _animator;


        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public async Awaitable FadeIn(Action onComplete = null)
        {
            _animator.CrossFadeInFixedTime(k_Open, 0.1f);

            while (true)
            {
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.IsTag(k_Open) && stateInfo.normalizedTime >= 1f)
                    break;

                await Awaitable.EndOfFrameAsync();
            }
        }

        public async Awaitable FadeOut(Action onComplete = null)
        {
            _animator.CrossFade(k_Close, .1f);
            while (true)
            {
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                if (stateInfo.IsTag(k_Close) && stateInfo.normalizedTime >= 1f)
                    break;

                await Awaitable.EndOfFrameAsync();
            }
        }
    }
}
