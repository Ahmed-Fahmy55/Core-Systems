using UnityEngine;

namespace Zone8.UI.TabSystem
{
    public class SimpleTab : TabBase
    {
        [SerializeField] private CanvasGroup _contentGroup;

        public override void ActivateContent()
        {
            if (_contentGroup == null) return;
            _contentGroup.alpha = 1;
            _contentGroup.blocksRaycasts = true;
            _contentGroup.interactable = true;
        }

        public override void DeactivateContent()
        {
            if (_contentGroup == null) return;
            _contentGroup.alpha = 0;
            _contentGroup.blocksRaycasts = false;
            _contentGroup.interactable = false;
        }

        public override void Highlight() { transform.localScale = Vector3.one * 1.2f; }
        public override void Dehighlight() { transform.localScale = Vector3.one; }
    }
}