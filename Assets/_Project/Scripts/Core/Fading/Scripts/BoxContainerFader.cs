using Michsky.UI.Heat;
using System;
using UnityEngine;
namespace Zone8.Fading
{
    [RequireComponent(typeof(BoxContainer))]
    public class BoxContainerFader : MonoBehaviour, IFader
    {
        BoxContainer boxContainer;

        private void Awake()
        {
            boxContainer = GetComponent<BoxContainer>();
            boxContainer.enabled = false;
        }

        public async Awaitable FadeIn(Action onComplete = null)
        {
            boxContainer.enabled = true;
            await Awaitable.WaitForSecondsAsync(boxContainer.cachedItems.Count * boxContainer.itemCooldown);
        }

        public async Awaitable FadeOut(Action onComplete = null)
        {
            boxContainer.enabled = false;
            await Awaitable.WaitForSecondsAsync(boxContainer.cachedItems.Count * boxContainer.itemCooldown);
        }
    }
}
