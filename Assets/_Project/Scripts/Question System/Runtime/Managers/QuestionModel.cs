using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Zone8.Events;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.Messages;
using Zone8.SOAP.AssetVariable;

namespace Zone8.Question.Runtime.Managers
{

    public class QuestionModel : MonoBehaviour
    {
        public event Action<QuestionBase> QuestionUpdated;

        [Header("Config")]
        [SerializeField] private AssetVariableRef<QuestionGameConfigSo> _gameConfig;

        private List<QuestionBase> _avilableQuestions;
        private List<QuestionBase> _askedQuestions = new();

        public int CurrentQuestionNumb { get; private set; }
        public AssetVariableRef<QuestionGameConfigSo> Config => _gameConfig;

        private bool _isConfigLoaded;

        private void Awake()
        {
            var handle = _gameConfig.LoadAssetAsync();

            handle.Completed += op =>
            {
                if (op.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    _isConfigLoaded = true;
                }
                else
                {
                    Logger.LogError("Failed to load game config asset.");
                }
            };
        }

        [Button]
        public void UpdateQuestion()
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (!_isConfigLoaded)
            {
                Logger.LogError("Game config asset is not loaded yet.");
                return;
            }

            if (_avilableQuestions == null) UpdateAvialbeQuestions();

            if (CurrentQuestionNumb >= _gameConfig.Asset.QuestionsNumb)
            {
                EventBus<QuestionsFinished>.Raise();
                _avilableQuestions = null;
                return;
            }

            if (CurrentQuestionNumb >= _avilableQuestions.Count)
            {
                Logger.LogError($"the current asked question number greater than available question: ending exam");
                EventBus<QuestionsFinished>.Raise();
                _avilableQuestions = null;
                return;
            }

            QuestionBase currentQuestion = _avilableQuestions[CurrentQuestionNumb];
            UpdateQuestion(currentQuestion);
            return;
        }

        public void UpdateQuestion(QuestionBase currentQuestion)
        {
            _askedQuestions.Add(currentQuestion);
            QuestionUpdated?.Invoke(currentQuestion);
            CurrentQuestionNumb++;
        }

        public int GetAskedQuestionsNumb()
        {
            return _askedQuestions.Count;
        }

        public void UpdateAvialbeQuestions()
        {
            _askedQuestions.Clear();
            CurrentQuestionNumb = 0;
            _avilableQuestions = _gameConfig.Asset.QuestionsList.Questions.ToList();

            if (_avilableQuestions.Count == 0)
            {
                Logger.LogError("No questions available for the selected type.");
                return;
            }

            if (_gameConfig.Asset.RandomizeQuestions)
            {
                _avilableQuestions.Shuffle();
            }

            if (_gameConfig.Asset.QuestionsNumb > _avilableQuestions.Count)
            {
                Logger.LogWarning($"The number of questions to be asked in the game is greater than the available questions.");
            }
        }
    }
}
