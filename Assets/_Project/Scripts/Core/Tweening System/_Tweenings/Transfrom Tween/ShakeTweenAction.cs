using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct ShakeTweenAction : ITweenAction
    {
        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Shake Settings", Order = 1)]
        [SerializeField] ETransformType _shakehType;

        [BoxGroup("Shake Settings", Order = 1)]
        [SerializeField] Vector3 _value;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("Indicates how much the punch will vibrate.")]
        [SerializeField] int _vibration;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). Setting it to 0 will shake along a single direction.\r\nNOTE: if you're shaking a single axis via the Vector3 strength parameter, randomness should be left to at least 90.")]
        [SerializeField] float _randomness;

        [BoxGroup("Shake Settings", Order = 1), ShowIf("_shakehType", ETransformType.Position)]
        [Tooltip(" If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool _snapping;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not.")]
        [SerializeField] bool _fadeout;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip(" The type of randomness to apply, Full (fully random) or Harmonic (more balanced and visually more pleasant).")]
        [SerializeField] ShakeRandomnessMode _randomnessMode;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }

            Tween tween;
            switch (_shakehType)
            {
                case ETransformType.Position:
                    tween = target.transform.DOShakePosition(CoreSettings.Duration, _value, _vibration, _randomness, _snapping, _fadeout, _randomnessMode);
                    break;
                case ETransformType.Rotation:
                    tween = target.transform.DOShakeRotation(CoreSettings.Duration, _value, _vibration, _randomness, _fadeout, _randomnessMode);
                    break;
                case ETransformType.Scale:
                    tween = target.transform.DOShakeScale(CoreSettings.Duration, _value, _vibration, _randomness, _fadeout, _randomnessMode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(_shakehType),
                        _shakehType,
                        "Unhandled ETransformType value"
                    );
            }

            CoreSettings.Apply(tween);

            return tween;
        }

    }
}
