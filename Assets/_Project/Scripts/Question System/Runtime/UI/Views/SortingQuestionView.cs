using System;
using System.Collections.Generic;
using UnityEngine;
using Zone8.Question.Core;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.UI.Answers;
using Zone8.Selection;

namespace Zone8.Question.Runtime.UI.Views
{
    public class SortingQuestionView : QuestionViewBase
    {

        [SerializeField] private UISubmitButton _submitButton;

        private List<SortingAnswerUI> _answers = new();
        private List<SortingAnswerUI> _selectedAnswers = new();
        public override Type SupportedQuestionType => typeof(SortingQuestion);


        protected override void Awake()
        {
            base.Awake();
            _selectionController.ItemSelected += OnItemSelected;
            _selectionController.ItemDeselected += OnItemDeselected;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _selectionController.ItemSelected -= OnItemSelected;
            _selectionController.ItemDeselected -= OnItemDeselected;
        }

        public override async Awaitable CleanQuestion()
        {
            for (int i = _answers.Count - 1; i >= 0; i--)
            {
                var answerUI = _answers[i];
                await answerUI.Fade(false);
                answerUI.Pool?.Release(answerUI);
            }
            _selectionController.ResetSelection();
        }

        protected override async Awaitable ShowFeedbackEffect(bool isTrue)
        {
            foreach (var answer in _answers)
            {
                await answer.HighlightAnswer(_answers.IndexOf(answer));
            }
        }

        protected override async Awaitable UpdateQuestionAnswers(QuestionBase question)
        {
            _answers.Clear();

            if (question is not SortingQuestion sortingQuestion)
            {
                Logger.LogError("Question is not a Sorting QUestion");
                return;
            }

            // Create a shuffled copy of the answers
            var shuffledAnswers = new List<QuestionAnswer>(question.Answers);
            shuffledAnswers.Shuffle();

            foreach (var answer in shuffledAnswers)
            {
                var answerUI = (SortingAnswerUI)GetAnswerUI(answer);
                answerUI.CorrectIndex = Array.IndexOf(question.Answers, answer);
                _answers.Add(answerUI);
                await answerUI.Fade(true);
            }

            _submitButton.SetButtonVisibility(true);
            _submitButton.HideOnNoSelection = false;
        }

        public override void OnQuestionAnswered(List<ISelectable> selected)
        {
            var allSelectables = new List<ISelectable>();
            foreach (var answer in _answers)
            {
                if (answer is ISelectable selectableAnswer)
                    allSelectables.Add(selectableAnswer);
            }

            selected = allSelectables;

            _submitButton.SetButtonVisibility(false);
            _submitButton.HideOnNoSelection = true;

            base.OnQuestionAnswered(selected);
        }

        private void OnItemDeselected(ISelectable selectable)
        {
            if (selectable is not SortingAnswerUI answerUI)
                return;

            if (_selectedAnswers.Contains(answerUI)) _selectedAnswers.Remove(answerUI);
        }

        /// <summary>
        /// Handles selection of SortingAnswerUI items for swapping.
        /// When two answers are selected, swaps their positions visually and in the answer list.
        /// </summary>
        private void OnItemSelected(ISelectable selectable)
        {
            if (selectable is not SortingAnswerUI selectedAnswer)
                return;

            if (_selectedAnswers.Count == 0)
            {
                _selectedAnswers.Add(selectedAnswer);
                return;
            }

            if (_selectedAnswers.Count == 1)
            {
                _selectedAnswers.Add(selectedAnswer);

                // Get the RectTransforms of the selected answers
                var firstTransform = _selectedAnswers[0].transform as RectTransform;
                var secondTransform = _selectedAnswers[1].transform as RectTransform;

                // Swap their sibling indices
                int firstIndex = firstTransform.GetSiblingIndex();
                int secondIndex = secondTransform.GetSiblingIndex();

                (_answers[firstIndex], _answers[secondIndex]) = (_answers[secondIndex], _answers[firstIndex]);

                firstTransform.SetSiblingIndex(secondIndex);
                secondTransform.SetSiblingIndex(firstIndex);

                _selectedAnswers.Clear();
                _selectionController.ResetSelection();
            }
        }
    }
}
