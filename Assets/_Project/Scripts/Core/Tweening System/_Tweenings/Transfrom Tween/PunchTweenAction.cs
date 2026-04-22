using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct PunchTweenAction : ITweenAction
    {

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Punch Settings", Order = 1)]
        [SerializeField] ETransformType _punchType;

        [BoxGroup("Punch Settings", Order = 1)]
        [SerializeField] Vector3 _value;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip("Indicates how much the punch will vibrate.")]
        [SerializeField] int _vibration;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip("epresents how much (0 to 1) the vector will go beyond the starting position when bouncing backwards. 1 creates a full oscillation between the punch direction and the opposite direction, while 0 oscillates only between the punch and the start position")]
        [SerializeField] float _elasticity;

        [BoxGroup("Punch Settings", Order = 1), ShowIf("_punchType", ETransformType.Position)]
        [Tooltip(" If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool _snapping;


        public Tween Act(GameObject target)
        {
            Tween tween;
            switch (_punchType)
            {
                case ETransformType.Position:
                    tween = target.transform.DOPunchPosition(_value, CoreSettings.Duration, _vibration, _elasticity, _snapping);
                    break;
                case ETransformType.Rotation:
                    tween = target.transform.DOPunchRotation(_value, CoreSettings.Duration, _vibration, _elasticity);
                    break;
                case ETransformType.Scale:
                    tween = target.transform.DOPunchScale(_value, CoreSettings.Duration, _vibration, _elasticity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(_punchType),
                        _punchType,
                        "Unhandled ETransformType value"
                    );
            }

            CoreSettings.Apply(tween);

            return tween;
        }

    }
}
