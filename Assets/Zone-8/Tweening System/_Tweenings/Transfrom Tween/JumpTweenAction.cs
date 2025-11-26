using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Tweening
{
    public struct JumpTweenAction : ITweenAction
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

        [BoxGroup("Jump Settings", Order = 1)]
        [SerializeField] private bool isLocal;

        [BoxGroup("Jump Settings", Order = 1)]
        [Tooltip("If true the end value will be calculated as start value + the given value")]
        [SerializeField] bool isValueRelative;

        [BoxGroup("Jump Settings", Order = 1)]
        [SerializeField] Vector3 value;

        [BoxGroup("Jump Settings", Order = 1)]
        [Tooltip(" Power of the jump (the max height of the jump is represented by this plus the final Y offset.")]
        [SerializeField] float jumpPower;

        [BoxGroup("Jump Settings", Order = 1)]
        [Tooltip("Total number of jumps.")]
        [SerializeField] int jumpNumbs;

        [BoxGroup("Jump Settings", Order = 1)]
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
                tween = target.transform.DOLocalJump(value, jumpPower, jumpNumbs, Duration, snapping);
            }
            else
            {
                tween = target.transform.DOJump(value, jumpPower, jumpNumbs, Duration, snapping);
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
