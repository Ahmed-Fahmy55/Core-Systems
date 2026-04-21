using UnityEngine;

namespace Zone8.Fading
{
    public interface IFader
    {
        Awaitable FadeIn(System.Action onComplete = null);

        Awaitable FadeOut(System.Action onComplete = null);
    }
}
