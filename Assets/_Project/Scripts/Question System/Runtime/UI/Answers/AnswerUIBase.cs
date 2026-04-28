using UnityEngine;
using UnityEngine.Pool;
using Zone8.Fading;
using Zone8.Question.Runtime.Base;
using Zone8.Selection;

namespace Zone8.Question.Runtime.UI.Answers
{

    public abstract class AnswerUIBase : UISelectableBase
    {
        protected QuestionAnswer _questionAnswer;
        protected IFader _fader;
        public IObjectPool<AnswerUIBase> Pool { get; private set; }
        public virtual QuestionAnswer GetAnswer() => _questionAnswer;

        private void Awake()
        {
            _fader = GetComponent<IFader>();
        }

        public virtual void Init(QuestionAnswer answer, IObjectPool<AnswerUIBase> pool)
        {
            _questionAnswer = answer;
            Pool = pool;

            if (_fader == null)
                _fader = GetComponent<IFader>();

            UpdateAnswerData(answer);
        }


        public async Awaitable Fade(bool isFadeIn)
        {
            if (_fader == null)
            {
                Logger.LogError("Fader component is missing on AnswerUIBase. Skipping fade.");
                return;
            }

            if (isFadeIn)
                await _fader.FadeIn();
            else
                await _fader.FadeOut();
        }

        public abstract Awaitable HighlightAnswer();

        public abstract void ResetAnswer();

        protected abstract void UpdateAnswerData(QuestionAnswer answer);
    }
}
