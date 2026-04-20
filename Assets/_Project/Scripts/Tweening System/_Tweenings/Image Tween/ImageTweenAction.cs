using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Tweening
{
    public class ImageTweenAction : ITweenAction
    {
        public enum EActionType { Color, Fade, Fill, GradientColor }

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


        ////////////////////////////////////////////////////////////

        [BoxGroup("Image Settings", Order = 1)]
        [SerializeField] EActionType actionType;

        [BoxGroup("Image Settings", Order = 1), ShowIf(nameof(actionType), EActionType.Color)]
        [SerializeField] Color ColorTo;

        [BoxGroup("Image Settings", Order = 1), ShowIf("@actionType == EActionType.Fade || actionType == EActionType.Fill")]
        [SerializeField] float toValue;

        [BoxGroup("Image Settings", Order = 1), ShowIf(nameof(actionType), EActionType.GradientColor)]
        [InfoBox("Changes the target's color via the given gradient.\r\nNOTE: Only uses the colors of the gradient, not the alphas.\r\nNOTE: Creates a Sequence, not a Tweener.")]
        [SerializeField] Gradient GradientTo;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Logger.LogError("Null target");
                return null;
            }
            if (target.TryGetComponent<Image>(out var image) == false)
            {
                Logger.LogError("Target does not have an Image component");
                return null;
            }
            Tween tween;

            switch (actionType)
            {
                case EActionType.Color:
                    tween = image.DOColor(ColorTo, Duration);
                    break;
                case EActionType.Fade:
                    tween = image.DOFade(toValue, Duration);
                    break;
                case EActionType.Fill:
                    tween = image.DOFillAmount(toValue, Duration);
                    break;
                case EActionType.GradientColor:
                    tween = image.DOGradientColor(GradientTo, Duration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                    nameof(actionType),
                    actionType,
                    "Unhandled EActionType value"
               );
            }

            tween.SetDelay(Delay).SetUpdate(UpdateType).SetAutoKill(AutoKill);

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
