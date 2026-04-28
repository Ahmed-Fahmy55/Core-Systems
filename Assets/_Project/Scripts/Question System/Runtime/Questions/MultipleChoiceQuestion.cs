using System.Linq;
using Zone8.Question.Runtime.Base;

namespace Zone8.Question.Core
{
    public class MultipleChoiceQuestion : QuestionBase
    {
        public override bool CheckAnswers(QuestionAnswer[] answers)
        {
            if (answers == null || answers.Length == 0)
            {
                Logger.LogError("Answers is null or empty");
                return false;
            }

            foreach (QuestionAnswer answer in answers)
            {
                if (answer == null)
                {
                    Logger.Log("Answers is null");
                    return false;
                }

                var actualAnswer = Answers.FirstOrDefault(a => a.ID == answer.ID);
                if (actualAnswer == null)
                {
                    Logger.LogError("Answers is not in Answers");
                    return false;
                }
            }

            int correctCount = GetCorrectAnswerCount();

            if (answers.Length > correctCount || answers.Length < correctCount) return false;

            foreach (var answer in answers)
            {
                if (!answer.IsCorrect) return false;
            }

            return true;
        }

        private int GetCorrectAnswerCount()
        {
            int count = 0;
            foreach (var answer in Answers)
            {
                if (answer.IsCorrect)
                {
                    count++;
                }
            }
            return count;
        }

    }
}
