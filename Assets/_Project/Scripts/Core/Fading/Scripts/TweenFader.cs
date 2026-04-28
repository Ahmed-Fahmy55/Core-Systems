using DG.Tweening;
using System;
using UnityEngine;
using Zone8.Tweening;

namespace Zone8.Fading
{
    [RequireComponent(typeof(SequenceActionExecuter))]
    public class TweenFader : MonoBehaviour, IFader
    {
        private SequenceActionExecuter _actionExecuter;
        private SequenceActionExecuter ActionExecuter => _actionExecuter ??= GetComponent<SequenceActionExecuter>();


        public async Awaitable FadeIn(Action onComplete = null)
        {
            ActionExecuter.Play();
            await Awaitable.WaitForSecondsAsync(ActionExecuter.Sequence.Duration());
        }

        public async Awaitable FadeOut(Action onComplete = null)
        {
            ActionExecuter.PlayBack();
            await Awaitable.WaitForSecondsAsync(ActionExecuter.Sequence.Duration());
        }
    }
}
