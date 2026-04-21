using UnityEngine;
using Zone8.Fading;

namespace Zone8.Screens
{
    [RequireComponent(typeof(CanvasGroup), typeof(IFader))]
    public abstract class UIScreenBase : MonoBehaviour, IUIScreen
    {
        private CanvasGroup _canvasGroup;
        private IFader _animator;
        private CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();


        private void Awake()
        {
            _animator = GetComponent<IFader>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual async Awaitable Show()
        {
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            await _animator.FadeIn();
            SetInteraction(true);
        }

        public virtual async Awaitable Hide()
        {
            SetInteraction(false);
            await _animator.FadeOut();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Enables or disables interaction with the screen.
        /// </summary>
        /// <param name="enable">True to enable interaction, false to disable.</param>
        public virtual void SetInteraction(bool enable)
        {
            CanvasGroup.blocksRaycasts = enable;
        }
    }
}
