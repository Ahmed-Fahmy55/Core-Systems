using TMPro;
using UnityEngine;
using Zone8.Question.Runtime.Base;

namespace Zone8.Question.Runtime.UI.Answers
{
    public class ChoiceAnswerUI : AnswerUIBase
    {
        private TextMeshProUGUI _answerText;


        protected virtual void Awake()
        {
            _answerText = GetComponentInChildren<TextMeshProUGUI>();
        }


        public override void ResetAnswer()
        {
            _answerText.color = Color.white; // Reset text color to default
        }


        protected override void UpdateAnswerData(QuestionAnswer answer)
        {
            _answerText.text = answer.AnswerText;
        }

        public override async Awaitable HighlightAnswer()
        {
            await Awaitable.EndOfFrameAsync();
            //TODO: Add highlight logic (color change, animation, etc.)
            _answerText.color = _questionAnswer.IsCorrect ? Color.green : Color.red;
        }

        protected override void OnSelect()
        {
            base.OnSelect();
            //TODO: Add selection Effect (e.g., disable button, change color, etc.)
            _answerText.color = Color.yellow; // Example: Change text color to gray on selection
        }
        protected override void OnDeselect()
        {
            base.OnDeselect();
            //Todo : Add deselection Effect (e.g., enable button, reset color, etc.)
            _answerText.color = Color.white; // Example: Reset text color to white on deselection
        }
    }
}
