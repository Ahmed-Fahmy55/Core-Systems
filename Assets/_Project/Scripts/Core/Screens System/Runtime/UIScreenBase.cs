using UnityEngine;
using Zone8.Fading;

namespace Zone8.Screens
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreenBase : MonoBehaviour, IUIScreen
    {
        private CanvasGroup _canvasGroup;
        private IFader _fader;
        private CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

        private void Awake()
        {
            // RequireComponent can't enforce interfaces, so the fader stays optional:
            // without one the screen still works, it just snaps instead of fading.
            _fader = GetComponent<IFader>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual async Awaitable Show()
        {
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            if (_fader != null) await _fader.FadeIn();
            SetInteraction(true);
        }

        public virtual async Awaitable Hide()
        {
            SetInteraction(false);
            if (_fader != null) await _fader.FadeOut();
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
