using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zone8.Question.Core;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.UI.Answers;
using Zone8.Selection;
using Zone8.Utilities;

namespace Zone8.Question.Runtime.UI.Views
{
    public class MatchChoiceQuestionView : QuestionViewBase
    {
        [SerializeField] private Transform _rightAnswersContainer;
        [SerializeField] private Transform _leftAnswersContainer;
        [SerializeField] private UILine _linePrefab;

        private List<MatchingAnswerUI> _answers = new();
        [ShowInInspector]
        private Dictionary<MatchingPair, GameObject> _lines = new Dictionary<MatchingPair, GameObject>();
        private List<MatchingAnswerUI> _selectedAnswers = new();
        private List<GameObject> _correctLineObjects = new List<GameObject>();

        public override Type SupportedQuestionType => typeof(MatchChoiceQuestion);


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
            foreach (var line in _lines.Values)
            {
                Destroy(line);
            }

            foreach (var line in _correctLineObjects)
            {
                Destroy(line);
            }

            _lines.Clear();
            _correctLineObjects.Clear();

            for (int i = _answers.Count - 1; i >= 0; i -= 2)
            {
                ChoiceAnswerUI answerUI = _answers[i];
                ChoiceAnswerUI answerUI2 = _answers[i - 1];

                answerUI.Fade(false);
                await answerUI2.Fade(false);

                answerUI.Pool?.Release(answerUI);
                answerUI2.Pool?.Release(answerUI2);
            }
            _selectionController.ResetSelection();
        }

        protected override async Awaitable ShowFeedbackEffect(bool isTrue)
        {
            if (_currentQuestion is not MatchChoiceQuestion matchQuestion)
            {
                Logger.LogError("Current question is not a MatchChoiceQuestion");
                return;
            }

            foreach (var pair in matchQuestion.CorrectPairs)
            {
                var leftUI = _answers.FirstOrDefault(a => a.GetAnswer().ID == pair.LeftItem.ID);
                var rightUI = _answers.FirstOrDefault(a => a.GetAnswer().ID == pair.RightItem.ID);

                if (leftUI == null || rightUI == null)
                {
                    Logger.LogError("Could not find matching UI elements for pair: " + pair.LeftItem.AnswerText);
                    continue;
                }
                _ = leftUI.HighlightAnswer();
                await rightUI.HighlightAnswer();

                UILine line = CreatLine(leftUI.LinkTargetRight, rightUI.LinkTargetLeft);
                line.color = Color.green;
                line.Thickness = 6f;
                _correctLineObjects.Add(line.gameObject);
            }
        }

        protected override async Awaitable UpdateQuestionAnswers(QuestionBase question)
        {
            _answers.Clear();
            MatchChoiceQuestion matchChoiceQuestion = question as MatchChoiceQuestion;

            if (matchChoiceQuestion == null)
            {
                Logger.LogError("Question is not a MultipleChoiceQuestion");
                return;
            }

            var leftItems = matchChoiceQuestion.LeftColumn;
            var rightItems = matchChoiceQuestion.RightColumn;

            leftItems.Shuffle();
            rightItems.Shuffle();

            for (int i = 0; i < leftItems.Count; i++)
            {
                var pair = matchChoiceQuestion.CorrectPairs.First(p => p.LeftItem.ID == leftItems[i].ID);

                // LEFT side
                var answerUILeft = GetAnswerUI(pair.LeftItem) as MatchingAnswerUI;
                answerUILeft.transform.SetParent(_leftAnswersContainer, false);
                answerUILeft.IsLeftSide = true;
                answerUILeft.Pair = pair;
                _answers.Add(answerUILeft);

                // RIGHT side
                var answerUIRight = GetAnswerUI(pair.RightItem) as MatchingAnswerUI;
                answerUIRight.transform.SetParent(_rightAnswersContainer, false);
                answerUIRight.IsLeftSide = false;
                answerUIRight.Pair = pair;
                _answers.Add(answerUIRight);

                answerUILeft.Fade(true);
                await answerUIRight.Fade(true);
            }
        }

        private void OnItemDeselected(ISelectable selectable)
        {
            if (selectable is not MatchingAnswerUI answerUI)
                return;

            // Find the associated MatchingPair in _lines
            var pairEntry = _lines.Keys.FirstOrDefault(pair =>
                pair.LeftItem == answerUI.GetAnswer() || pair.RightItem == answerUI.GetAnswer());

            if (pairEntry.LeftItem != null && _lines.TryGetValue(pairEntry, out var lineObj))
            {
                // Deselect the other answer in the pair
                QuestionAnswer otherAnswer = pairEntry.LeftItem == answerUI.GetAnswer()
                    ? pairEntry.RightItem
                    : pairEntry.LeftItem;

                var otherAnswerUI = _answers.FirstOrDefault(a => a.GetAnswer() == otherAnswer);
                if (otherAnswerUI != null && otherAnswerUI.IsSelected)
                    otherAnswerUI.Deselect();

                Destroy(lineObj);
                _lines.Remove(pairEntry);
            }

            _selectedAnswers.Remove(answerUI);
        }

        private void OnItemSelected(ISelectable selectable)
        {
            var selected = selectable as MatchingAnswerUI;
            if (selected == null)
                return;

            if (_selectedAnswers.Count == 1 && selected.IsLeftSide == _selectedAnswers[0].IsLeftSide)
            {
                selected.Deselect();
                Logger.LogError("same side answer selected");
                return;
            }

            _selectedAnswers.Add(selected);
            if (_selectedAnswers.Count == 2)
            {
                // Always find left and right, regardless of selection order
                var left = _selectedAnswers.FirstOrDefault(a => a.IsLeftSide);
                var right = _selectedAnswers.FirstOrDefault(a => !a.IsLeftSide);

                UILine line = CreatLine(left.LinkTargetRight, right.LinkTargetLeft);

                var pair = new MatchingPair(left.GetAnswer(), right.GetAnswer());
                _lines.Add(pair, line.gameObject);

                _selectedAnswers.Clear();
            }
        }

        private UILine CreatLine(RectTransform start, RectTransform end)
        {
            var line = Instantiate(_linePrefab, _answersContainer);
            line.SetPoints(start, end);
            return line;
        }
    }
}

