using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct JumpTweenAction : ITweenAction
    {

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Jump Settings", Order = 1)]
        [SerializeField] private bool _isLocal;

        [BoxGroup("Jump Settings", Order = 1)]
        [SerializeField] Vector3 _value;

        [BoxGroup("Jump Settings", Order = 1)]
        [Tooltip(" Power of the jump (the max height of the jump is represented by this plus the final Y offset.")]
        [SerializeField] float _jumpPower;

        [BoxGroup("Jump Settings", Order = 1)]
        [Tooltip("Total number of jumps.")]
        [SerializeField] int _jumpNumbs;

        [BoxGroup("Jump Settings", Order = 1)]
        [Tooltip("If TRUE the tween will smoothly snap all values to integers..")]
        [SerializeField] bool _snapping;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }

            Tween tween;
            if (_isLocal)
            {
                tween = target.transform.DOLocalJump(_value, _jumpPower, _jumpNumbs, CoreSettings.Duration, _snapping);
            }
            else
            {
                tween = target.transform.DOJump(_value, _jumpPower, _jumpNumbs, CoreSettings.Duration, _snapping);
            }

            CoreSettings.Apply(tween);

            return tween;
        }
    }

}
