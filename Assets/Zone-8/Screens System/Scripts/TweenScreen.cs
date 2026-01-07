using Zone8.Tweening;
using DG.Tweening;
using UnityEngine;

namespace Zone8.Screens
{
    [RequireComponent(typeof(SequenceActionExecuter))]
    public class TweenScreen : ScreenBase
    {
        private SequenceActionExecuter _actionExecuter;
        private SequenceActionExecuter ActionExecuter => _actionExecuter ??= GetComponent<SequenceActionExecuter>();

        private Sequence _sequence;



        /// <summary>
        /// Starts the hide effect for the screen using the sequence action executer.
        /// </summary>
        public override async Awaitable StartHideEffect()
        {
            if (_sequence == null) return;

            ActionExecuter.PlayBack();
            await Awaitable.WaitForSecondsAsync(_sequence.Duration());
        }

        /// <summary>
        /// Starts the show effect for the screen using the sequence action executer.
        /// </summary>
        public override async Awaitable StartShowEffect()
        {
            _sequence = ActionExecuter.Play();
            if (_sequence == null) return;

            await Awaitable.WaitForSecondsAsync(_sequence.Duration());
        }
    }
}
