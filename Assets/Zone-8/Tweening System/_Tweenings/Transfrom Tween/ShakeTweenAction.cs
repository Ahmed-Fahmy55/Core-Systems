using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct ShakeTweenAction : ITweenAction
    {
        #region CoreSettings
        [field: SerializeField, BoxGroup("Core Settings")]
        public float Duration { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public float Delay { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool Loop { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), ShowIf(nameof(Loop))]
        public int LoopCount { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), ShowIf(nameof(Loop))]
        public LoopType LoopType { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool CustomEase { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), ShowIf(nameof(CustomEase))]
        public AnimationCurve EaseCurve { get; set; }

        [field: SerializeField, BoxGroup("Core Settings"), HideIf(nameof(CustomEase))]
        public Ease Ease { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public UpdateType UpdateType { get; set; }

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool AutoKill { get; set; }
        #endregion

        /////////////////////////////////////////////////////

        [BoxGroup("Shake Settings", Order = 1)]
        [SerializeField] ETransformType shakehType;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("If true the end value will be calculated as start value + the given value")]
        [SerializeField] bool isValueRelative;

        [BoxGroup("Shake Settings", Order = 1)]
        [SerializeField] Vector3 value;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("Indicates how much the punch will vibrate.")]
        [SerializeField] int vibration;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). Setting it to 0 will shake along a single direction.\r\nNOTE: if you're shaking a single axis via the Vector3 strength parameter, randomness should be left to at least 90.")]
        [SerializeField] float randomness;

        [BoxGroup("Shake Settings", Order = 1), ShowIf("shakehType", ETransformType.Position)]
        [Tooltip(" If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool snapping;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip("If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not.")]
        [SerializeField] bool fadeout;

        [BoxGroup("Shake Settings", Order = 1)]
        [Tooltip(" The type of randomness to apply, Full (fully random) or Harmonic (more balanced and visually more pleasant).")]
        [SerializeField] ShakeRandomnessMode randomnessMode;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }

            Tween tween;
            switch (shakehType)
            {
                case ETransformType.Position:
                    tween = target.transform.DOShakePosition(Duration, value, vibration, randomness, snapping, fadeout, randomnessMode);
                    break;
                case ETransformType.Rotation:
                    tween = target.transform.DOShakeRotation(Duration, value, vibration, randomness, fadeout, randomnessMode);
                    break;
                case ETransformType.Scale:
                    tween = target.transform.DOShakeScale(Duration, value, vibration, randomness, fadeout, randomnessMode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(shakehType),
                        shakehType,
                        "Unhandled ETransformType value"
                    );
            }

            tween.SetDelay(Delay).SetUpdate(UpdateType).SetAutoKill(AutoKill).SetRelative(isValueRelative);

            if (CustomEase)
            {
                tween.SetEase(EaseCurve);
            }
            else
            {
                tween.SetEase(Ease);
            }

            if (Loop) tween.SetLoops(LoopCount, LoopType);

            return tween;
        }

    }
}
