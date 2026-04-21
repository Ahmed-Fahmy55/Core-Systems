using UnityEngine;
using UnityEngine.Events;

namespace Zone8.Screens
{
    public class Screen : UIScreenBase
    {
        public UnityEvent ScreenShowed;
        public UnityEvent ScreenHidden;


        public override async Awaitable Show()
        {
            await base.Show();
            ScreenShowed?.Invoke();
        }

        public override async Awaitable Hide()
        {
            await base.Hide();
            ScreenHidden?.Invoke();
        }
    }
}
