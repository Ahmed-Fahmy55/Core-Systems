using DG.Tweening;
using UnityEngine;
using Zone8.Tweening;

namespace Zone8.Fading
{
    [RequireComponent(typeof(SequenceActionExecuter))]
    public class TweenFader : MonoBehaviour, IFader
    {
        private SequenceActionExecuter _actionExecuter;
        private SequenceActionExecuter ActionExecuter => _actionExecuter ??= GetComponent<SequenceActionExecuter>();

        public async Awaitable FadeIn()
        {
            await Awaitable.EndOfFrameAsync();
            ActionExecuter.Play();
            await Awaitable.WaitForSecondsAsync(ActionExecuter.Sequence.Duration());
        }

        public async Awaitable FadeOut()
        {
            await Awaitable.EndOfFrameAsync();
            ActionExecuter.PlayBack();
            await Awaitable.WaitForSecondsAsync(ActionExecuter.Sequence.Duration());
        }
    }
}
