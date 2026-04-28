using System;

namespace Zone8.Question.Runtime.Base
{
    [Serializable]
    public class QuestionAnswer
    {
        public string ID;
        public string AnswerText;
        public bool IsCorrect;

        public QuestionAnswer(string id, string answerText, bool isTrue)
        {
            ID = id;
            AnswerText = answerText;
            IsCorrect = isTrue;
        }
    }
}
