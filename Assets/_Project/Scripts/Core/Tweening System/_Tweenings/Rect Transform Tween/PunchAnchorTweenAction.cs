using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{

    [Serializable]
    public struct PunchAnchorTweenAction : ITweenAction
    {

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Punch Settings", Order = 1)]
        [SerializeField] Vector2 _value;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip("Indicates how much the punch will vibrate.")]
        [SerializeField] int _vibration;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip(" Represents how much (0 to 1) the vector will go beyond the starting position when bouncing backwards. 1 creates a full oscillation between the punch direction and the opposite direction, while 0 oscillates only between the punch and the start position.")]
        [SerializeField] float _elasticity;

        [BoxGroup("Punch Settings", Order = 1)]
        [Tooltip("If TRUE the tween will smoothly snap all values to integers.")]
        [SerializeField] bool _snapping;


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
            tween = rectTransform.DOPunchAnchorPos(_value, CoreSettings.Duration, _vibration, _elasticity, _snapping);
            CoreSettings.Apply(tween);

            return tween;
        }
    }
}
