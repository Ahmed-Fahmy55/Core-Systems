using UnityEngine;

namespace Zone8.Screens
{
    interface IUIScreen
    {
        Awaitable Show();
        Awaitable Hide();
    }
}
