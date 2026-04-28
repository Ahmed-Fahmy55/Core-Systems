using System.Linq;
using Zone8.Question.Runtime.Base;

namespace Zone8.Question.Core
{
    public class SingleChoiceQuestion : QuestionBase
    {
        public override bool CheckAnswers(QuestionAnswer[] answers)
        {
            if (answers == null || answers.Length == 0)
            {
                Logger.LogError("Answers is null or empty");
                return false;
            }

            if (answers.Length > 1)
            {
                Logger.LogError("Answers length is greater than 1");
                return false;
            }

            if (answers[0] == null)
            {
                Logger.LogError("Answers is null");
                return false;
            }

            var selectedAnswer = answers[0];
            var actualAnswer = Answers.FirstOrDefault(a => a.ID == selectedAnswer.ID);

            return actualAnswer != null && actualAnswer.IsCorrect;
        }
    }
}
