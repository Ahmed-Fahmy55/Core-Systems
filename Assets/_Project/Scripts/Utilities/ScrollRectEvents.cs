using System.Collections;
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

        private void Start()
        {
            StartCoroutine(CheckImmediateEnd());
        }

        private IEnumerator CheckImmediateEnd()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();

            var viewport = _scrollRect.viewport.rect.height;
            var content = _scrollRect.content.rect.height;

            // If content is same size or smaller → we're already "at the end"
            if (content <= viewport)
            {
                EndReached?.Invoke();
            }
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
