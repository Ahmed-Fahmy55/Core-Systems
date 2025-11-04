namespace Zone8.Fading
{
    public interface IFader
    {
        void FadeIn(float duration = 0, System.Action onComplete = null);

        void FadeOut(float duration = 0, System.Action onComplete = null);
    }
}
