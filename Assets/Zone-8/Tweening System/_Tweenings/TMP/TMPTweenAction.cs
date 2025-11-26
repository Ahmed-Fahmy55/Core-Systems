using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

namespace Zone8.Tweening
{
    public class TMPTweenAction : ITweenAction
    {
        enum EActionType { Color, Fade, Scale, Text, FontSize }

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


        [BoxGroup("TMP Settings", Order = 1)]
        [SerializeField] EActionType actionType;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@actionType == EActionType.Color ")]
        [SerializeField] Color toColor;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@actionType == EActionType.Fade || actionType == EActionType.FontSize")]
        [SerializeField] float toValue;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@actionType == EActionType.Text")]
        [SerializeField] string toText;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@actionType == EActionType.Scale")]
        [SerializeField] Vector3 toScaleValue;

        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Logger.LogError("Null target");
                return null;
            }

            if (!target.TryGetComponent(out TextMeshProUGUI tmp))
            {
                Logger.LogError("Target does not have a TextMeshProUGUI component");
                return null;
            }
            Tween tween;

            switch (actionType)
            {
                case EActionType.Color:
                    tween = tmp.DOColor(toColor, Duration);
                    break;
                case EActionType.Fade:
                    tween = tmp.DOFade(toValue, Duration);
                    break;
                case EActionType.Scale:
                    tween = tmp.DOScale(toScaleValue, Duration);
                    break;
                case EActionType.Text:
                    tween = tmp.DoText(toText, Duration);
                    break;
                case EActionType.FontSize:
                    tween = tmp.DOFontSize(toValue, Duration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                    nameof(actionType),
                    actionType,
                    "Unhandled actionType value"
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
