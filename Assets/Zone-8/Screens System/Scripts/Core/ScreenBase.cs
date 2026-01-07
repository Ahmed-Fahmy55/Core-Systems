using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Zone8.Screens
{

    public interface IScreen
    {
        Awaitable Show();
        Awaitable Hide();
        void EnableInteraction(bool enable);
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ScreenBase : MonoBehaviour, IScreen
    {
        [field: SerializeField] public bool HideCurrent { get; private set; } = true;
        [field: SerializeField] public bool AutoHide { get; private set; }

        [ShowIf(nameof(AutoHide))]
        [SerializeField] protected float _hideDelay = 1f;


        public bool IsVisible { get; private set; }


        public UnityEvent ScreenShowed;
        public UnityEvent ScreenHidden;

        private CanvasGroup _canvasGroup;
        private CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

        /// <summary>
        /// Shows the screen with a transition effect.
        /// </summary>
        public async Awaitable Show()
        {
            gameObject.SetActive(true);
            await StartShowEffect();
            IsVisible = true;
            transform.SetAsLastSibling();
            EnableInteraction(true);
            ScreenShowed?.Invoke();

        }

        /// <summary>
        /// Hides the screen with a transition effect.
        /// </summary>
        public async Awaitable Hide()
        {
            if (AutoHide)
            {
                await Awaitable.WaitForSecondsAsync(_hideDelay);
            }
            EnableInteraction(false);
            await StartHideEffect();
            IsVisible = false;
            gameObject.SetActive(false);
            ScreenHidden?.Invoke();
        }

        /// <summary>
        /// Enables or disables interaction with the screen.
        /// </summary>
        /// <param name="enable">True to enable interaction, false to disable.</param>
        public void EnableInteraction(bool enable)
        {
            CanvasGroup.blocksRaycasts = enable;
        }

        /// <summary>
        /// Starts the hide effect for the screen.
        /// Must be implemented by derived classes.
        /// </summary>
        public abstract Awaitable StartHideEffect();

        /// <summary>
        /// Starts the show effect for the screen.
        /// Must be implemented by derived classes.
        /// </summary>
        public abstract Awaitable StartShowEffect();
    }
}
