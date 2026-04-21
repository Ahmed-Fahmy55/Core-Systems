using DG.Tweening;
using System;
using UnityEngine;
using Zone8.Tweening;

namespace Zone8.Fading
{
    [RequireComponent(typeof(SequenceActionExecuter))]
    public class TweenAnimator : MonoBehaviour, IFader
    {
        private SequenceActionExecuter _actionExecuter;
        private SequenceActionExecuter ActionExecuter => _actionExecuter ??= GetComponent<SequenceActionExecuter>();

        private Sequence _sequence;



        public async Awaitable FadeIn(Action onComplete = null)
        {
            _sequence = ActionExecuter.Play();
            if (_sequence == null) return;

            await Awaitable.WaitForSecondsAsync(_sequence.Duration());
        }

        public async Awaitable FadeOut(Action onComplete = null)
        {
            if (_sequence == null) return;

            ActionExecuter.PlayBack();
            await Awaitable.WaitForSecondsAsync(_sequence.Duration());
        }
    }
}
