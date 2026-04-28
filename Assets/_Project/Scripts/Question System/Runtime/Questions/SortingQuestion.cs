using Zone8.Question.Runtime.Base;

namespace Zone8.Question.Core
{
    public class SortingQuestion : QuestionBase
    {
        public override bool CheckAnswers(QuestionAnswer[] answers)
        {
            if (answers == null || answers.Length == 0)
            {
                Logger.LogError("Answers is null or empty");
                return false;
            }

            if (answers.Length != Answers.Length)
            {
                Logger.LogError("Answers length dosent match than question answers");
                return false;
            }

            for (int i = 0; i < Answers.Length; i++)
            {
                if (answers[i] == null)
                {
                    Logger.LogError("Answer at index " + i + " is null");
                    return false;
                }

                if (answers[i].ID != Answers[i].ID)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
