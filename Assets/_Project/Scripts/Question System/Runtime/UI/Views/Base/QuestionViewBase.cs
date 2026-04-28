using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Zone8.Multiplayer.ConnectionManagement;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.Data;
using Zone8.Question.Runtime.UI.Answers;
using Zone8.Selection;
using Zone8.SOAP.ScriptableVariable;

namespace Zone8.Question.Runtime.UI.Views
{
    /// <summary>
    /// Base class for handling the UI representation of a question within the question system.
    /// Provides shared logic for displaying question data, managing answer elements,
    /// handling player selections, and synchronizing multiplayer submissions.
    /// </summary>
    [RequireComponent(typeof(SelectionController), typeof(CanvasGroup))]
    public abstract class QuestionViewBase : NetworkBehaviour
    {
        #region === Events ===

        /// <summary>
        /// Invoked when the local player answers a question.
        /// </summary>
        /// <param name="QuestionBase">The current question.</param>
        /// <param name="QuestionAnswer[]">The player's chosen answers.</param>
        /// <param name="bool">Whether the answer is correct.</param>
        /// <param name="EFeedbackType">The feedback display type.</param>
        public event Action<QuestionBase, QuestionAnswer[], bool> QuestionAnswered;

        /// <summary>
        /// Invoked when all players have submitted their answers in a multiplayer session.
        /// </summary>
        public event Action<Dictionary<ulong, PlayerAnswers>> PlayerSubmissionsReached;

        #endregion

        #region === Serialized Fields ===

        [Header("Settings")]
        [SerializeField]
        protected ScriptableVariableRef<EFeedbackType> _feedbackType;

        [Header("UI References")]
        [SerializeField, Tooltip("Prefab used to create individual answer UI elements.")]
        protected AnswerUIBase _answerPrefab;

        [SerializeField, Tooltip("Container where all answer UI elements are instantiated.")]
        protected Transform _answersContainer;

        [SerializeField, Tooltip("Text component displaying the question text.")]
        protected TextMeshProUGUI _questionText;

        [SerializeField, Tooltip("Optional image displayed with the question.")]
        protected Image _questionImage;

        protected SelectionController _selectionController;

        #endregion

        #region === Properties ===

        public SelectionController SelectionController => _selectionController ??= GetComponent<SelectionController>();

        public abstract Type SupportedQuestionType { get; }

        #endregion

        #region === Protected Members ===

        protected QuestionBase _currentQuestion;
        protected CanvasGroup _canvasGroup;
        protected IObjectPool<AnswerUIBase> _answersPool;
        protected readonly Dictionary<ulong, PlayerAnswers> _playerAnswersDic = new();

        #endregion

        #region === Unity Lifecycle ===

        /// <summary>
        /// Initializes internal references, event bindings, and answer pooling.
        /// </summary>
        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _answersPool = new ObjectPool<AnswerUIBase>(
                CreateAnswer, OnGetAnswer, OnReleaseAnswer);

            SelectionController.SelectionCompleted += OnQuestionAnswered;
        }

        /// <summary>
        /// Cleans up listeners and pooled elements when destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
            SelectionController.SelectionCompleted -= OnQuestionAnswered;
        }

        #endregion

        #region === Question Lifecycle ===

        /// <summary>
        /// Updates the UI when a new question is presented.
        /// Clears previous data, resets selection state, and repopulates answer elements.
        /// </summary>
        public virtual async Awaitable OnQuestionUpdated(QuestionBase question)
        {
            ResetSelectionControllerRpc();
            _playerAnswersDic.Clear();

            UpdateQuestion(question);
            await UpdateQuestionAnswers(question);
        }

        /// <summary>
        /// Locks the UI to prevent user interaction.
        /// </summary>
        public virtual void LockUI() => _canvasGroup.blocksRaycasts = false;

        /// <summary>
        /// Unlocks the UI, allowing player interaction.
        /// </summary>
        public virtual void UnlockUI() => _canvasGroup.blocksRaycasts = true;

        /// <summary>
        /// Updates the visible question text and image.
        /// </summary>
        protected virtual void UpdateQuestion(QuestionBase question)
        {
            _currentQuestion = question;
            _questionText.text = question.QuestionText;

            if (question.Image != null)
            {
                _questionImage.sprite = question.Image;
                _questionImage.gameObject.SetActive(true);
            }
            else
            {
                _questionImage.gameObject.SetActive(false);
            }
        }

        #endregion

        #region === Answer Handling ===

        /// <summary>
        /// Handles the event when the player finishes selecting an answer.
        /// </summary>
        public virtual void OnQuestionAnswered(List<ISelectable> selected)
        {
            QuestionAnswer[] questionAnswers = null;

            if (selected != null && selected.Count > 0)
            {
                var answers = selected.OfType<AnswerUIBase>().ToArray();
                questionAnswers = new QuestionAnswer[answers.Length];
                for (int i = 0; i < answers.Length; i++)
                    questionAnswers[i] = answers[i].GetAnswer();
            }

            bool isCorrect = _currentQuestion.CheckAnswers(questionAnswers);
            QuestionAnswered?.Invoke(_currentQuestion, questionAnswers, isCorrect);

            SubmitAnswerRpc(CreateNetAnswer(questionAnswers, isCorrect), NetworkManager.Singleton.LocalClientId);
        }

        /// <summary>
        /// Spawns or retrieves an AnswerUI element from the pool.
        /// </summary>
        protected virtual AnswerUIBase GetAnswerUI(QuestionAnswer answer)
        {
            AnswerUIBase answerUI = _answersPool.Get();
            answerUI.Init(answer, _answersPool);
            SelectionController.AddSelectable(answerUI);
            return answerUI;
        }

        /// <summary>
        /// Called when an answer is fetched from the pool.
        /// </summary>
        protected virtual void OnGetAnswer(AnswerUIBase ui) => ui.gameObject.SetActive(true);

        /// <summary>
        /// Called when an answer is released back into the pool.
        /// </summary>
        protected virtual void OnReleaseAnswer(AnswerUIBase ui)
        {
            ui.ResetAnswer();
            ui.gameObject.SetActive(false);
        }

        /// <summary>
        /// Instantiates a new AnswerUI element.
        /// </summary>
        protected virtual AnswerUIBase CreateAnswer() => Instantiate(_answerPrefab, _answersContainer);

        #endregion

        #region === Networking ===

        /// <summary>
        /// Called by clients to submit their answers to the server.
        /// </summary>
        [Rpc(SendTo.Server)]
        private void SubmitAnswerRpc(PlayerAnswers playerAnswers, ulong clientID)
        {
            if (!IsServer)
                return;

            var data = SessionManager<PlayerSessionQuestionData>.Instance.GetPlayerData(clientID);
            if (data != null)
                playerAnswers.PlayerIndex = data.Value.PlayerIndex;

            _playerAnswersDic.TryAdd(clientID, playerAnswers);

            if (_playerAnswersDic.Count >= NetworkManager.Singleton.ConnectedClients.Count)
            {
                PlayerSubmissionsReached?.Invoke(_playerAnswersDic);
            }
        }

        /// <summary>
        /// Resets selection controller state across all clients.
        /// </summary>
        [Rpc(SendTo.Everyone)]
        private void ResetSelectionControllerRpc() => SelectionController.ResetSelection();

        /// <summary>
        /// Creates a serializable PlayerAnswers struct for network transmission.
        /// </summary>
        private PlayerAnswers CreateNetAnswer(QuestionAnswer[] questionAnswers, bool isCorrect)
        {
            var answers = new PlayerAnswers();

            if (questionAnswers == null || questionAnswers.Length == 0)
                return answers;

            string[] ids = new string[questionAnswers.Length];
            for (int i = 0; i < ids.Length; i++)
                ids[i] = questionAnswers[i].ID;

            answers.AnswersId = ids;
            answers.IsTrue = isCorrect;
            return answers;
        }

        #endregion

        public async Awaitable ShowFeedback(bool isTrue)
        {
            if (_feedbackType.Value == EFeedbackType.ShowOnQuestion ||
                _feedbackType.Value == EFeedbackType.ShowBoth)
            {
                Logger.Log("Showing feedback effect for question.");
                await ShowFeedbackEffect(isTrue);
            }
        }

        public abstract Awaitable CleanQuestion();
        protected abstract Awaitable UpdateQuestionAnswers(QuestionBase question);
        protected abstract Awaitable ShowFeedbackEffect(bool isTrue);

    }

    /// <summary>
    /// Represents a player's answer submission for networking.
    /// Serialized across the network using Unity Netcode.
    /// </summary>
    public struct PlayerAnswers : INetworkSerializable
    {
        public int PlayerIndex;
        public string[] AnswersId;
        public bool IsTrue;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerIndex);
            serializer.SerializeValue(ref IsTrue);

            int length = AnswersId != null ? AnswersId.Length : 0;
            serializer.SerializeValue(ref length);

            if (serializer.IsReader)
                AnswersId = new string[length];

            for (int i = 0; i < length; i++)
                serializer.SerializeValue(ref AnswersId[i]);
        }
    }
}
