using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{

    [Serializable]
    public struct ScaleTweenAction : ITweenAction
    {
        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Scale Settings", Order = 1)]
        [SerializeField] Vector3 _value;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }

            Tween tween;
            tween = target.transform.DOScale(_value, CoreSettings.Duration);

            CoreSettings.Apply(tween);

            return tween;
        }
    }
}
