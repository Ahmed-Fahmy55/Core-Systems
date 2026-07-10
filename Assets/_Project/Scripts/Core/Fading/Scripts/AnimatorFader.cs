using UnityEngine;

namespace Zone8.Fading
{
    /// <summary>
    /// Fader driven by an Animator with "Open"/"Close" states. The states must also be
    /// tagged "Open"/"Close" — the tag is how completion is detected.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorFader : MonoBehaviour, IFader
    {
        private const string k_Open = "Open";
        private const string k_Close = "Close";
        private const float k_crossFadeSeconds = 0.1f;
        private const float k_maxWaitSeconds = 5f;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public async Awaitable FadeIn() => await PlayAndWait(k_Open);

        public async Awaitable FadeOut() => await PlayAndWait(k_Close);

        private async Awaitable PlayAndWait(string state)
        {
            _animator.CrossFadeInFixedTime(state, k_crossFadeSeconds);

            // Let the crossfade actually begin before sampling — the previous state's
            // finished clip would otherwise satisfy the exit check instantly.
            await Awaitable.NextFrameAsync();

            float deadline = Time.realtimeSinceStartup + k_maxWaitSeconds;
            while (Time.realtimeSinceStartup < deadline)
            {
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsTag(state) && stateInfo.normalizedTime >= 1f)
                    return;

                await Awaitable.EndOfFrameAsync();
            }

            Logger.LogWarning($"[AnimatorFader] Timed out waiting for state '{state}' on '{name}'. " +
                              "Check that the state exists and is tagged accordingly.", this);
        }
    }
}
