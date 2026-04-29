using UnityEngine;

namespace Zone8.Question.Runtime.UI.Answers
{
    public class SortingAnswerUI : ChoiceAnswerUI
    {
        public int CorrectIndex { get; set; }

        public async Awaitable HighlightAnswer(int index)
        {
            _answerText.color = index == CorrectIndex ? Color.green : Color.red;
        }
    }
}
