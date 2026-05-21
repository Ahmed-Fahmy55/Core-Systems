using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zone8.Question.Core;
using Zone8.Question.Runtime.Base;
using Zone8.Question.Runtime.UI.Answers;
using Zone8.Selection;
using Zone8.Utilities;

namespace Zone8.Question.Runtime.UI.Views
{
    public class SortingQuestionView : QuestionViewBase
    {

        private List<SortingAnswerUI> _answers = new();
        public override Type SupportedQuestionType => typeof(SortingQuestion);

        private LayoutGroupFreezer _layoutFreezer;

        private Vector3[] _slotLocalPositions;
        private Vector2[] _slotAnchoredPositions;
        private Canvas _canvas;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponentInParent<Canvas>();
            _layoutFreezer = _answersContainer.GetComponent<LayoutGroupFreezer>();
        }

        public void NotifyDragging(SortingAnswerUI draggedItem)
        {
            float draggedY = draggedItem.transform.position.y;
            int currentVisualIndex = -1;

            for (int i = 0; i < _answers.Count; i++)
            {
                Vector3 worldTargetPos = _layoutFreezer.transform.TransformPoint(_slotLocalPositions[i]);

                if (i == 0 && draggedY > worldTargetPos.y)
                {
                    currentVisualIndex = 0;
                    break;
                }
                if (i == _answers.Count - 1 && draggedY < worldTargetPos.y)
                {
                    currentVisualIndex = _answers.Count - 1;
                    break;
                }
                if (i < _answers.Count - 1)
                {
                    Vector3 worldNextPos = _layoutFreezer.transform.TransformPoint(_slotLocalPositions[i + 1]);

                    if (draggedY <= worldTargetPos.y && draggedY >= worldNextPos.y)
                    {
                        currentVisualIndex = (Mathf.Abs(draggedY - worldTargetPos.y) < Mathf.Abs(draggedY - worldNextPos.y)) ? i : i + 1;
                        break;
                    }
                }
            }

            if (currentVisualIndex != -1)
            {
                int oldIndex = _answers.IndexOf(draggedItem);
                if (oldIndex != currentVisualIndex && oldIndex != -1)
                {
                    _answers.RemoveAt(oldIndex);
                    _answers.Insert(currentVisualIndex, draggedItem);

                    AnimateRearrangedItems(draggedItem);
                }
            }
        }

        public void NotifyStoppedDragging(SortingAnswerUI draggedItem)
        {
            int finalIndex = _answers.IndexOf(draggedItem);
            if (finalIndex != -1)
            {
                Vector2 targetAnchoredPos = _slotAnchoredPositions[finalIndex];
                draggedItem.RectTransform.DOKill(true);
                draggedItem.RectTransform.DOAnchorPos(targetAnchoredPos, 0.2f).SetEase(Ease.OutBack);
            }
        }


        public float GetCanvasScale() => _canvas.scaleFactor;

        public override async Awaitable FadeOut()
        {
            for (int i = _answers.Count - 1; i >= 0; i--)
            {
                var answerUI = _answers[i];
                await answerUI.Fade(false);
                answerUI.Pool?.Release(answerUI);
            }
            _selectionController.ResetSelection();

            await base.FadeOut();
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
                Logger.LogError("Question is not a Sorting Question");
                return;
            }

            var shuffledAnswers = new List<QuestionAnswer>(question.Answers);
            shuffledAnswers.Shuffle();

            if (_layoutFreezer != null)
                _layoutFreezer.SetLayoutEnabled(true);

            foreach (var answer in shuffledAnswers)
            {
                var answerUI = (SortingAnswerUI)GetAnswerUI(answer);
                answerUI.Initialize(this);
                answerUI.CorrectIndex = Array.IndexOf(question.Answers, answer);
                _answers.Add(answerUI);
                await answerUI.Fade(true);
            }

            await Awaitable.NextFrameAsync();
            RectTransform containerRect = _answersContainer.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            await Awaitable.NextFrameAsync();

            _slotLocalPositions = new Vector3[_answers.Count];
            _slotAnchoredPositions = new Vector2[_answers.Count];
            for (int i = 0; i < _answers.Count; i++)
            {
                _slotLocalPositions[i] = _answers[i].RectTransform.localPosition;
                _slotAnchoredPositions[i] = _answers[i].RectTransform.anchoredPosition;
            }
            if (_layoutFreezer != null)
                _layoutFreezer.SetLayoutEnabled(false);
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

            base.OnQuestionAnswered(selected);
        }
        private void AnimateRearrangedItems(SortingAnswerUI draggedItem)
        {
            for (int i = 0; i < _answers.Count; i++)
            {
                if (_answers[i] == draggedItem) continue;

                Vector2 targetAnchoredPos = _slotAnchoredPositions[i];

                _answers[i].RectTransform.DOKill(true);
                _answers[i].RectTransform.DOAnchorPos(targetAnchoredPos, 0.25f).SetEase(Ease.OutQuad);
            }
        }
    }
}