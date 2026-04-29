using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zone8.Events;
using Zone8.Multiplayer.Utilities;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.Messages;
using Zone8.Question.Runtime.UI.Views;
using Zone8.Selection;
using Zone8.SOAP.AssetVariable;

namespace Zone8.Question.Runtime.Managers
{
    [RequireComponent(typeof(QuestionModel))]
    public class QuestionPresenter : NetworkBehaviour
    {
        public Action<QuestionBase, QuestionAnswer[], bool> QuestionAnswered;
        public Action QuestionsFinished;
        public Action QuestionUpdated;


        [Header("Settings")]
        [Tooltip("Set true to make the presenter responsable for updating question depending on feedback")]
        [SerializeField] protected bool _autoUpdateQuestion = true;

        [Header("Questions View")]
        [SerializeField] protected QuestionViewBase[] _questionUis;

        [Header("UI Refs")]
        [SerializeField] protected NetworkTimer _timer;
        [SerializeField] protected UISubmitButton _submitButton;

        protected AssetVariableRef<QuestionGameConfigSo> _gameConfigSo;
        protected Dictionary<Type, QuestionViewBase> _questionUiMap;
        protected QuestionModel _questionModel;
        protected QuestionViewBase _currentQuestionView;
        protected EventBinding<QuestionsFinished> _examFinishd;
        protected bool _roundStarted;

        #region Unity Methods

        protected virtual void Awake()
        {
            _questionModel = GetComponent<QuestionModel>();
            _examFinishd = new EventBinding<QuestionsFinished>(OnQuestionsFinished);
        }

        protected virtual void OnEnable()
        {
            EventBus<QuestionsFinished>.Register(_examFinishd);
            _questionModel.QuestionUpdated += OnQuestionUpdated;
            _timer.TimerFinished.AddListener(OnQuestionTimeOut);
            SubscribeToQuestionUiEvents(true);
        }

        protected virtual void OnDisable()
        {
            _questionModel.QuestionUpdated -= OnQuestionUpdated;
            EventBus<QuestionsFinished>.Deregister(_examFinishd);
            _timer.TimerFinished.RemoveListener(OnQuestionTimeOut);
            SubscribeToQuestionUiEvents(false);
        }

        protected virtual void Start()
        {
            _gameConfigSo = _questionModel.Config;

            if (_gameConfigSo.Source == AssetSource.Addressable)
            {
                var handle = _gameConfigSo.LoadAssetAsync();
                handle.Completed += op =>
                {
                    if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    {
                        if (_gameConfigSo.Asset.SelectedTimeMode == ETimeMode.NoTimeLimit) _timer.HideUI();
                    }
                    else
                    {
                        Logger.LogError("Failed to load QuestionGameConfigSo addressable asset.");
                    }
                };
            }
            else
            {
                if (_gameConfigSo.Asset.SelectedTimeMode == ETimeMode.NoTimeLimit) _timer.HideUI();
            }


            _questionUiMap = new();
            foreach (QuestionViewBase questionUi in _questionUis)
            {
                _questionUiMap.Add(questionUi.SupportedQuestionType, questionUi);
            }
        }

        #endregion


        public void UpdateQuestion()
        {
            _questionModel.UpdateQuestion();
        }

        #region Private Methods

        private void SubscribeToQuestionUiEvents(bool subscribe)
        {
            foreach (var ui in _questionUis)
            {
                if (ui == null) continue;
                if (subscribe)
                    ui.QuestionAnswered += OnQuestionAnswered;
                else
                    ui.QuestionAnswered -= OnQuestionAnswered;
            }
        }

        protected async Awaitable ShowQuestionUi(QuestionViewBase questionUi, QuestionBase question)
        {
            _currentQuestionView = questionUi;
            _currentQuestionView.LockUI();
            questionUi.gameObject.SetActive(true);
            await questionUi.OnQuestionUpdated(question);
            _submitButton.SetSelectionController(questionUi.SelectionController);
            _currentQuestionView?.UnlockUI();
        }

        protected void UpdateQuestionTimer()
        {
            switch (_gameConfigSo.Asset.SelectedTimeMode)
            {
                case ETimeMode.PerQuestion:
                    _timer.StartTimer(_gameConfigSo.Asset.Time);
                    break;

                case ETimeMode.PerExam:
                    if (!_roundStarted)
                    {
                        _timer.StartTimer(_gameConfigSo.Asset.Time);
                        _roundStarted = true;
                    }
                    else
                    {
                        _timer.ResumeTimer();
                    }
                    break;
            }
        }

        #endregion

        #region Event Handlers

        protected virtual async void OnQuestionUpdated(QuestionBase question)
        {
            try
            {
                await HideActiveQuestionViewAsync();


                if (_questionUiMap.TryGetValue(question.GetType(), out var ui) && ui != null)
                {
                    await ShowQuestionUi(ui, question);
                }
                else
                {
                    Logger.LogError($"No UI found for question type: {question.GetType()}");
                    return;
                }

                UpdateQuestionTimer();
                QuestionUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

        }

        protected virtual async void OnQuestionAnswered(QuestionBase question, QuestionAnswer[] answers, bool isTrue)
        {
            _currentQuestionView?.LockUI();
            _submitButton.SetButtonVisibility(false);
            _timer.PauseTimer();

            QuestionAnswered?.Invoke(question, answers, isTrue);
            await ShowFeedback(isTrue);

            if (_autoUpdateQuestion) _questionModel.UpdateQuestion();
        }


        protected async Awaitable ShowFeedback(bool isTrue)
        {
            if (_currentQuestionView != null)
            {
                await _currentQuestionView.ShowFeedback(isTrue);
            }

            await Awaitable.WaitForSecondsAsync(_gameConfigSo.Asset.FeedbackTime);
        }

        protected virtual void OnQuestionTimeOut()
        {
            if (_gameConfigSo.Asset.SelectedTimeMode == ETimeMode.PerQuestion)
            {
                _currentQuestionView?.OnQuestionAnswered(null);
            }
            else
            {
                OnQuestionsFinished();
            }
        }

        protected virtual void OnQuestionsFinished()
        {
            if (!IsServer) return;
            _timer.PauseTimer();
            QuestionsFinished?.Invoke();
        }

        protected virtual async Awaitable HideActiveQuestionViewAsync()
        {
            if (_currentQuestionView != null)
            {
                _currentQuestionView.LockUI();
                await _currentQuestionView?.CleanQuestion();
                _currentQuestionView.gameObject.SetActive(false);
            }
        }

        #endregion
    }
}