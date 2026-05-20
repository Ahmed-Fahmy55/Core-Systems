using UnityEngine;
using UnityEngine.EventSystems;
using Zone8.Question.Runtime.UI.Views;

namespace Zone8.Question.Runtime.UI.Answers
{
    public class SortingAnswerUI : ChoiceAnswerUI, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private SortingQuestionView _view;

        public RectTransform RectTransform { get; private set; }
        public int CorrectIndex { get; set; }

        public void Initialize(SortingQuestionView view)
        {
            _view = view;
            RectTransform = GetComponent<RectTransform>();

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_view == null) return;
            transform.SetAsLastSibling();

            // add any visual feedback for dragging here (e.g., change color, scale, etc.)
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_view == null) return;

            RectTransform.anchoredPosition += eventData.delta / _view.GetCanvasScale();
            _view.NotifyDragging(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_view == null) return;

            _view.NotifyStoppedDragging(this);
        }

        public async Awaitable HighlightAnswer(int index)
        {
            _answerText.color = index == CorrectIndex ? Color.green : Color.red;
            await Awaitable.NextFrameAsync();
        }

        protected override void OnSelect()
        {
        }

        protected override void OnDeselect()
        {
        }
    }
}