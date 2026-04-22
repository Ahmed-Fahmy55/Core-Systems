using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{

    [Serializable]
    public struct AnchorJumpTweenAction : ITweenAction
    {

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }


        [BoxGroup("Jump Settings", Order = 1)]
        [SerializeField] Vector2 _value;

        [BoxGroup("Jump Settings", Order = 1)]
        [SerializeField] float _jumpPower;

        [BoxGroup("Jump Settings", Order = 1)]
        [SerializeField] int _jumpsNumb;

        [BoxGroup("Jump Settings", Order = 1)]
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
            tween = rectTransform.DOJumpAnchorPos(_value, _jumpPower, _jumpsNumb, CoreSettings.Duration, _snapping);
            CoreSettings.Apply(tween);

            return tween;
        }
    }
}
