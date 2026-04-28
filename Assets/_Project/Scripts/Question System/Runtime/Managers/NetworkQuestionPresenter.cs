using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zone8.Events;
using Zone8.Multiplayer.Utilities;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.Messages;
using Zone8.Question.Runtime.UI.Views;

namespace Zone8.Question.Runtime.Managers
{
    public class NetworkQuestionPresenter : QuestionPresenter
    {
        public event Action<Dictionary<ulong, PlayerAnswers>, QuestionBase> PlayerSubmissionsReached;


        private int _clientsReadyCount = 0;
        private bool _hasAnswerd;
        private bool _isAnswerTrue;
        private QuestionBase _currentQuestion;
        private NetworkVariable<NetworkString> _currentQuesstionID = new();
        private EventBinding<ResetGameEvent> _resetGameBind;

        protected override void Awake()
        {
            base.Awake();
            _resetGameBind = new EventBinding<ResetGameEvent>(OnGameReset);
            EventBus<ResetGameEvent>.Register(_resetGameBind);
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer && !IsHost) _submitButton.gameObject.SetActive(false);

            //Late spawn handling for clients to sync with current question
            if (!IsHost && IsClient)
            {
                if (!string.IsNullOrEmpty(_currentQuesstionID.Value))
                {
                    QuestionBase question = _gameConfigSo.Asset.QuestionsList.GetQuestionById(_currentQuesstionID.Value);
                    _ = UpdateUI(question);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            EventBus<ResetGameEvent>.Deregister(_resetGameBind);
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (var ui in _questionUis)
            {
                if (ui == null) continue;
                ui.PlayerSubmissionsReached += OnAllPlayersSubmitted;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            foreach (var ui in _questionUis)
            {
                if (ui == null) continue;
                ui.PlayerSubmissionsReached -= OnAllPlayersSubmitted;
            }
        }


        protected override void OnQuestionUpdated(QuestionBase question)
        {
            if (!IsServer) return;

            _currentQuestion = question;
            _currentQuesstionID.Value = question.ID;
            UpdateQuestionRpc(question.ID);
        }

        protected override void OnQuestionAnswered(QuestionBase question, QuestionAnswer[] answers, bool isTrue)
        {
            _currentQuestionView?.LockUI();
            _submitButton.SetButtonVisibility(false);
            _hasAnswerd = true;
            _isAnswerTrue = isTrue;
            QuestionAnswered?.Invoke(question, answers, isTrue);
        }


        private async void OnAllPlayersSubmitted(Dictionary<ulong, PlayerAnswers> playersData)
        {
            _timer.PauseTimer();
            PlayerSubmissionsReached?.Invoke(playersData, _currentQuestion);
            GoToNextQuestionRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void UpdateQuestionRpc(string questionId)
        {
            _hasAnswerd = false;
            QuestionBase question = _gameConfigSo.Asset.QuestionsList.GetQuestionById(questionId);
            _ = RunUiSequenceAsync(question);
        }

        [Rpc(SendTo.Everyone)]

        private void GoToNextQuestionRpc()
        {
            _ = RunNextQuestionSequence();
        }

        private async Awaitable RunNextQuestionSequence()
        {
            await ShowFeedback(_isAnswerTrue);
            if (_autoUpdateQuestion)
                _questionModel.UpdateQuestion();
        }

        protected override void OnQuestionTimeOut()
        {
            if (!_hasAnswerd && _gameConfigSo.Asset.SelectedTimeMode == ETimeMode.PerQuestion)
            {
                _currentQuestionView?.OnQuestionAnswered(null);
            }
            else if (_gameConfigSo.Asset.SelectedTimeMode == ETimeMode.PerExam)
            {
                OnQuestionsFinished();
            }
        }

        private void OnGameReset()
        {
            _questionModel.UpdateAvialbeQuestions();
            _currentQuestionView?.CleanQuestion();
        }

        private async Awaitable RunUiSequenceAsync(QuestionBase question)
        {
            await UpdateUI(question);
            NotifyUiFinishedRpc();
        }

        private async Awaitable UpdateUI(QuestionBase question)
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
        }

        [Rpc(SendTo.Server)]
        private void NotifyUiFinishedRpc()
        {
            _clientsReadyCount++;

            if (_clientsReadyCount >= NetworkManager.Singleton.ConnectedClients.Count)
            {
                UpdateQuestionTimer();
                QuestionUpdated?.Invoke();
                _clientsReadyCount = 0;
            }
        }
    }
}
