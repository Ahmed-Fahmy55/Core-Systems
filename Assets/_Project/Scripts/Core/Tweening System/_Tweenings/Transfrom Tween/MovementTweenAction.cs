using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{

    [Serializable]
    public struct MovementTweenAction : ITweenAction
    {
        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Movement Settings", Order = 1)]
        [SerializeField] private bool _isLocal;

        [BoxGroup("Movement Settings", Order = 1)]
        [SerializeField] Vector3 _value;

        [BoxGroup("Movement Settings", Order = 1)]
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
                tween = target.transform.DOLocalMove(_value, CoreSettings.Duration, _snapping);
            }
            else
            {
                tween = target.transform.DOMove(_value, CoreSettings.Duration, _snapping);
            }

            CoreSettings.Apply(tween);
            return tween;
        }
    }
}
