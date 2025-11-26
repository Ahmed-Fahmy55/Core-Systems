using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{

    [Serializable]
    public struct MovementTweenAction : ITweenAction
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

        [BoxGroup("Movement Settings", Order = 1)]
        [SerializeField] private bool isLocal;

        [BoxGroup("Movement Settings", Order = 1)]
        [Tooltip("If true the end value will be calculated as start value + the given value")]
        [SerializeField] bool isValueRelative;

        [BoxGroup("Movement Settings", Order = 1)]
        [SerializeField] Vector3 value;

        [BoxGroup("Movement Settings", Order = 1)]
        [Tooltip("If TRUE the tween will smoothly snap all values to integers..")]
        [SerializeField] bool snapping;

        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null");
                return null;
            }


            Tween tween;
            if (isLocal)
            {
                tween = target.transform.DOLocalMove(value, Duration, snapping);
            }
            else
            {
                tween = target.transform.DOMove(value, Duration, snapping);
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
