using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct PunchTweenAction : ITweenAction
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

        [BoxGroup("Punch Settings", Order = 1)]
        [SerializeField] ETransformType punchType;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip("If true the end value will be calculated as start value + the given value")]
        [SerializeField] bool isValueRelative;

        [BoxGroup("Punch Settings", Order = 1)]
        [SerializeField] Vector3 value;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip("Indicates how much the punch will vibrate.")]
        [SerializeField] int vibration;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip("epresents how much (0 to 1) the vector will go beyond the starting position when bouncing backwards. 1 creates a full oscillation between the punch direction and the opposite direction, while 0 oscillates only between the punch and the start position")]
        [SerializeField] float elasticity;

        [BoxGroup("Punch Settings", Order = 1), ShowIf("punchType", ETransformType.Position)]
        [Tooltip(" If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool snapping;

        public Tween Act(GameObject target)
        {
            Tween tween;
            switch (punchType)
            {
                case ETransformType.Position:
                    tween = target.transform.DOPunchPosition(value, Duration, vibration, elasticity, snapping);
                    break;
                case ETransformType.Rotation:
                    tween = target.transform.DOPunchRotation(value, Duration, vibration, elasticity);
                    break;
                case ETransformType.Scale:
                    tween = target.transform.DOPunchScale(value, Duration, vibration, elasticity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(punchType),
                        punchType,
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
