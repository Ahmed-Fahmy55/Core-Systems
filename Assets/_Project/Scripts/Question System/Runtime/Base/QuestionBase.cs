using System;
using UnityEngine;

namespace Zone8.Question.Runtime.Base
{
    [Serializable]
    public abstract class QuestionBase : ScriptableObject
    {
        public string ID;

        public CategorySo Category;

        public string QuestionText;

        public string CorrectFeedback;

        public string WrongFeedback;

        public Sprite Image;

        public EDifficultyLevel DifficultyLevel = EDifficultyLevel.Medium;

        public QuestionAnswer[] Answers;

        public abstract bool CheckAnswers(QuestionAnswer[] answers);

    }
}
