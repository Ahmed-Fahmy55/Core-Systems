using UnityEngine;
using Zone8.Question.Core;

namespace Zone8.Question.Runtime.UI.Answers
{
    public class MatchingAnswerUI : ChoiceAnswerUI
    {
        [field: SerializeField] public RectTransform LinkTargetLeft { get; private set; }
        [field: SerializeField] public RectTransform LinkTargetRight { get; private set; }
        public bool IsLeftSide { get; set; }

        public MatchingPair Pair { get; set; }

        private void Start()
        {
            LinkTargetRight.gameObject.SetActive(false);
            LinkTargetLeft.gameObject.SetActive(false);
        }

        protected override void OnSelect()
        {
            if (IsLeftSide)
            {
                LinkTargetRight.gameObject.SetActive(true);
            }
            else
            {
                LinkTargetLeft.gameObject.SetActive(true);
            }
        }

        protected override void OnDeselect()
        {
            if (IsLeftSide)
            {
                LinkTargetRight.gameObject.SetActive(false);
            }
            else
            {
                LinkTargetLeft.gameObject.SetActive(false);
            }
        }

        public override async Awaitable HighlightAnswer()
        {
            OnSelect();
        }
    }
}
