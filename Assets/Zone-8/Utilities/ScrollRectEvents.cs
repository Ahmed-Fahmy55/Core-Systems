using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Zone8.Utilities
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectEvents : MonoBehaviour
    {
        public UnityEvent EndReached;
        private ScrollRect _scrollRect;


        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            if (value.y <= .01f)
            {
                EndReached?.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_scrollRect != null)
            {
                _scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            }
        }
    }
}
