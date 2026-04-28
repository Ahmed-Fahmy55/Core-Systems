using System;
using System.Collections.Generic;
using UnityEngine;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.UI.Answers;

namespace Zone8.Question.Runtime.UI.Views
{
    public abstract class ChoiceQuestionViewBase<TQuestion> : QuestionViewBase
        where TQuestion : QuestionBase
    {
        protected readonly List<AnswerUIBase> _answers = new();

        public override Type SupportedQuestionType => typeof(TQuestion);

        protected override async Awaitable ShowFeedbackEffect(bool isTrue)
        {
            foreach (var answer in _answers)
                await answer.HighlightAnswer();
        }

        protected override async Awaitable UpdateQuestionAnswers(QuestionBase question)
        {
            _answers.Clear();

            if (question is not TQuestion targetQuestion)
            {
                Logger.LogError($"Question is not of expected type {typeof(TQuestion).Name}");
                return;
            }

            foreach (var answer in targetQuestion.Answers)
            {
                var answerUI = GetAnswerUI(answer);
                _answers.Add(answerUI);
                await answerUI.Fade(true);
            }
        }

        public override async Awaitable CleanQuestion()
        {
            for (int i = _answers.Count - 1; i >= 0; i--)
            {
                var answerUI = _answers[i];
                await answerUI.Fade(false);
                answerUI.Pool?.Release(answerUI);
            }
        }
    }
}
