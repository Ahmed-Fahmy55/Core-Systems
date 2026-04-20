using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Zone8.Utilities
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectEvents : MonoBehaviour
    {
        public UnityEvent _endReached;

        private ScrollRect _scrollRect;


        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        private void Start()
        {
            // Wait for UI layout to be fully calculated
            StartCoroutine(CheckImmediateEnd());
        }

        private IEnumerator CheckImmediateEnd()
        {
            // Wait one frame so layout elements finish sizing
            yield return null;
            Canvas.ForceUpdateCanvases();

            var viewport = _scrollRect.viewport.rect.height;
            var content = _scrollRect.content.rect.height;

            // If content is same size or smaller → we're already "at the end"
            if (content <= viewport)
            {
                _endReached?.Invoke();
            }
        }

        private void OnScrollValueChanged(Vector2 value)
        {
            if (value.y <= .01f)
            {
                _endReached?.Invoke();
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
