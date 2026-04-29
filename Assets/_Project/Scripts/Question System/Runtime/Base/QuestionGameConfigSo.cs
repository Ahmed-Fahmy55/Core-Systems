using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zone8.SOAP.ScriptableVariable;


namespace Zone8.Question.Runtime.Base
{
    [CreateAssetMenu(menuName = "Questions/Question Game Config")]
    public class QuestionGameConfigSo : ScriptableObject
    {

        [Title("Time Settings")]
        [EnumToggleButtons]
        [HideLabel, HorizontalGroup("GameMode")]
        public ETimeMode SelectedTimeMode = ETimeMode.PerQuestion;

        [ShowIf("@SelectedTimeMode==ETimeMode.PerQuestion||SelectedTimeMode==ETimeMode.PerExam")]
        [SuffixLabel("seconds", true)]
        [MinValue(0)]
        public float Time = 10f;


        [Title("Scoring Settings")]
        [ToggleLeft]
        public bool IsFixedScore = true;

        [ShowIf(nameof(IsFixedScore))]
        [MinValue(0)]
        public int Score = 10;

        [HideIf(nameof(IsFixedScore))]
        [TableList]
        public List<ScoreSettings> ScoreSettingsList;

        [Title("Feedback Settings")]
        [InlineProperty]
        [HideLabel]
        public ScriptableVariableRef<EFeedbackType> FeedbackType;

        [SuffixLabel("seconds", true)]
        [MinValue(0)]
        public float FeedbackTime = 2f;


        [Title("Question Data")]
        [MinValue(1)]
        public int QuestionsNumb = 5;
        public bool RandomizeQuestions;
        public QuestionsListSo QuestionsList;

        public int GetTotalQuestionsScore()
        {
            int score = 0;
            if (IsFixedScore)
            {
                score = Score * QuestionsNumb;
            }
            else
            {
                foreach (var question in QuestionsList.Questions)
                {
                    score += ScoreSettingsList.FirstOrDefault(s => s.difficultyLevel == question.DifficultyLevel).score;
                }
            }
            return score;
        }
        public int GetQuestionScore(QuestionBase question)
        {
            if (IsFixedScore)
            {
                return Score;
            }
            else
            {
                return ScoreSettingsList.FirstOrDefault(s => s.difficultyLevel == question.DifficultyLevel).score;
            }
        }
    }

    [System.Serializable]
    public struct ScoreSettings
    {
        public EDifficultyLevel difficultyLevel;
        public int score;
    }

    public enum ETimeMode
    {
        NoTimeLimit,
        PerQuestion,
        PerExam
    }

    public enum EQuestionType
    {
        None,
        MultipleChoice,
        SingleChoice,
        Matching,
        Sorting
    }

    public enum EFeedbackType
    {
        Hide,
        ShowMessage,
        ShowOnQuestion,
        ShowBoth
    }

    public enum EDifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public struct QuestionFeedback
    {
        public string QuestionId;
        public int Score;
        public string[] ChoiceIds;
    }
}
