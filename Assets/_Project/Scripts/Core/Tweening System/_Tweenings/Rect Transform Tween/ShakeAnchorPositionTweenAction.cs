using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{

    [Serializable]
    public struct ShakeAnchorTweenAction : ITweenAction
    {

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Shake Settings", Order = 1)]
        [SerializeField] Vector2 _value;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("Indicates how much the punch will vibrate.")]
        [SerializeField] int _vibration;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip(" Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). Setting it to 0 will shake along a single direction.")]
        [SerializeField] float _randomness;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool _snapping;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("(default: true) If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not.")]
        [SerializeField] bool _fadeout;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip(" (default: Full) The type of randomness to apply, Full (fully random) or Harmonic (more balanced and visually more pleasant).")]
        [SerializeField] ShakeRandomnessMode _shakeMode;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Logger.LogError("Null target");
                return null;
            }
            if (target.TryGetComponent<RectTransform>(out var rectTransform) == false)
            {
                Logger.LogError("Target does not have a RectTransform component");
                return null;
            }

            Tween tween;
            tween = rectTransform.DOShakeAnchorPos(CoreSettings.Duration, _value, _vibration, _randomness,
                _snapping, _fadeout, _shakeMode);

            CoreSettings.Apply(tween);

            return tween;
        }
    }
}
