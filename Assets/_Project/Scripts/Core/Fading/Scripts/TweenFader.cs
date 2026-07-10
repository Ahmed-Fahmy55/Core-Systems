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
            await WaitForSequence();
        }

        public async Awaitable FadeOut()
        {
            await Awaitable.EndOfFrameAsync();
            ActionExecuter.PlayBack();
            await WaitForSequence();
        }

        // Track the sequence itself instead of waiting a fixed scaled-time duration:
        // stays correct for unscaled tweens, altered timeScale, or a paused game.
        private async Awaitable WaitForSequence()
        {
            Sequence sequence = ActionExecuter.Sequence;
            if (sequence == null) return;

            while (sequence.IsActive() && sequence.IsPlaying())
                await Awaitable.NextFrameAsync();
        }
    }
}
