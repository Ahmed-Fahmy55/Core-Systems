using Michsky.UI.Heat;
using UnityEngine;

namespace Zone8.Fading
{
    [RequireComponent(typeof(BoxContainer))]
    public class BoxContainerFader : MonoBehaviour, IFader
    {
        private BoxContainer _boxContainer;

        private void Awake()
        {
            _boxContainer = GetComponent<BoxContainer>();
            _boxContainer.enabled = false;
        }

        public async Awaitable FadeIn()
        {
            _boxContainer.enabled = true;
            await Awaitable.WaitForSecondsAsync(_boxContainer.cachedItems.Count * _boxContainer.itemCooldown);
        }

        public async Awaitable FadeOut()
        {
            _boxContainer.enabled = false;
            await Awaitable.WaitForSecondsAsync(_boxContainer.cachedItems.Count * _boxContainer.itemCooldown);
        }
    }
}
