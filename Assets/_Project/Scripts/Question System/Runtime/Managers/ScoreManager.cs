using System;
using System.Linq;
using UnityEngine;
using Zone8.Events;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.Messages;
using Zone8.SOAP.AssetVariable;
using Zone8.SOAP.ScriptableVariable;

namespace Zone8.Question.Runtime.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] protected AssetVariableRef<QuestionGameConfigSo> _gameConfigSo;
        [SerializeField] protected ScriptableVariableRef<int> _scoreSv;

        protected QuestionPresenter _questionPresenter;
        private EventBinding<ResetGameEvent> _gameResetEvent;

        protected virtual void Awake()
        {
            _gameResetEvent = new EventBinding<ResetGameEvent>(ResetScore);
            _questionPresenter = FindAnyObjectByType<QuestionPresenter>();

            ResetScore();
        }

        protected virtual void OnEnable()
        {
            EventBus<ResetGameEvent>.Register(_gameResetEvent);
            _questionPresenter.QuestionAnswered += OnQuestionAnswerd;
        }

        protected virtual void OnDisable()
        {
            EventBus<ResetGameEvent>.Deregister(_gameResetEvent);
            _questionPresenter.QuestionAnswered -= OnQuestionAnswerd;
        }

        protected virtual void OnQuestionAnswerd(QuestionBase question, QuestionAnswer[] answers, bool isTrue)
        {
            QuestionFeedback feedback = new QuestionFeedback
            {
                QuestionId = question.ID,
            };


            feedback.Score = isTrue ? _gameConfigSo.Asset.GetQuestionScore(question) : 0;
            feedback.ChoiceIds = answers?.Select(a => a.ID).ToArray() ?? Array.Empty<string>();
            SaveFeedback(feedback);

            if (!isTrue) return;
            _scoreSv.Value += feedback.Score;
        }

        protected virtual void ResetScore()
        {
            if (!_scoreSv.IsNull) _scoreSv.Value = 0;
        }

        protected virtual void SaveFeedback(QuestionFeedback feedback)
        {
            /// Override this method to save feedback data (e.g., to a file, server, or analytics system).
        }

    }
}
