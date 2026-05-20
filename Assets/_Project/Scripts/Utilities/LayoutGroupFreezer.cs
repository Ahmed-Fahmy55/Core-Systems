using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Utilities
{
    [RequireComponent(typeof(LayoutElement))]
    [RequireComponent(typeof(RectTransform))]
    public class LayoutGroupFreezer : MonoBehaviour
    {
        private LayoutGroup _layout;
        private LayoutElement _element;
        private RectTransform _rect;

        private void Awake()
        {
            _layout = GetComponent<LayoutGroup>();
            _element = GetComponent<LayoutElement>();
            _rect = GetComponent<RectTransform>();

            if (_layout == null)
            {
                Logger.LogWarning($"[LayoutGroupFreezer] No Horizontal or Vertical LayoutGroup found on {gameObject.name}!");
            }
        }

        /// <summary>
        /// Controls the layout group state. When disabling, it locks current layout dimensions 
        /// into the LayoutElement to preserve sizes inside parent layouts.
        /// </summary>
        public void SetLayoutEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                _element.preferredWidth = -1;
                _element.preferredHeight = -1;

                if (_layout != null)
                    _layout.enabled = true;
            }
            else
            {
                Canvas.ForceUpdateCanvases();

                _element.preferredWidth = _rect.rect.width;
                _element.preferredHeight = _rect.rect.height;

                if (_layout != null)
                    _layout.enabled = false;
            }
        }
    }
}