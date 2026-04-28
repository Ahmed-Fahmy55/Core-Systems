using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Zone8.Question.Core;

namespace Zone8.Question.Runtime.Base
{
    [CreateAssetMenu(menuName = "Questions/Question List")]
    public class QuestionsListSo : SerializedScriptableObject
    {
        public List<QuestionBase> Questions;


        public QuestionBase GetQuestionById(string Id)
        {
            foreach (var question in Questions)
            {
                if (string.Equals(Id, question.ID))
                {
                    return question;
                }
            }
            Logger.LogError($"Question with ID {Id} not found.");
            return null;
        }

        public List<QuestionBase> GetQuestionsByType(EQuestionType questionType)
        {
            List<QuestionBase> filteredQuestions = new List<QuestionBase>();

            switch (questionType)
            {
                case EQuestionType.MultipleChoice:
                    filteredQuestions = GetQuestionsByType<MultipleChoiceQuestion>();
                    break;

                case EQuestionType.SingleChoice:
                    filteredQuestions = GetQuestionsByType<SingleChoiceQuestion>();
                    break;


                case EQuestionType.Matching:
                    filteredQuestions = GetQuestionsByType<MatchChoiceQuestion>();
                    break;

                case EQuestionType.Sorting:
                    filteredQuestions = GetQuestionsByType<SortingQuestion>();
                    break;

                default:
                    Logger.LogError($"Unsupported question type: {questionType}");
                    break;

            }
            return filteredQuestions;
        }

        private List<QuestionBase> GetQuestionsByType<T>() where T : QuestionBase
        {
            List<QuestionBase> filteredQuestions = new List<QuestionBase>();

            foreach (var question in Questions)
            {
                if (question is T)
                {
                    filteredQuestions.Add(question);
                }
            }
            return filteredQuestions;
        }
    }
}
