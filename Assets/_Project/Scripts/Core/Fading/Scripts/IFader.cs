using UnityEngine;

namespace Zone8.Fading
{
    public interface IFader
    {
        Awaitable FadeIn();
        Awaitable FadeOut();
    }
}
