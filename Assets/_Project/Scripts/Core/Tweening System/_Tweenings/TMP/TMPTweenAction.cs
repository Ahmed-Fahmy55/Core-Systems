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

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }


        [BoxGroup("TMP Settings", Order = 1)]
        [SerializeField] EActionType _actionType;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@_actionType == EActionType.Color ")]
        [SerializeField] Color _toColor;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@_actionType == EActionType.Fade || _actionType == EActionType.FontSize")]
        [SerializeField] float _toValue;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@_actionType == EActionType.Text")]
        [SerializeField] string _toText;

        [BoxGroup("TMP Settings", Order = 1)]
        [ShowIf("@_actionType == EActionType.Scale")]
        [SerializeField] Vector3 _toScaleValue;


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

            switch (_actionType)
            {
                case EActionType.Color:
                    tween = tmp.DOColor(_toColor, CoreSettings.Duration);
                    break;
                case EActionType.Fade:
                    tween = tmp.DOFade(_toValue, CoreSettings.Duration);
                    break;
                case EActionType.Scale:
                    tween = tmp.DOScale(_toScaleValue, CoreSettings.Duration);
                    break;
                case EActionType.Text:
                    tween = tmp.DoText(_toText, CoreSettings.Duration);
                    break;
                case EActionType.FontSize:
                    tween = tmp.DOFontSize(_toValue, CoreSettings.Duration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                    nameof(_actionType),
                    _actionType,
                    "Unhandled actionType value"
               );
            }

            CoreSettings.Apply(tween);

            return tween;

        }
    }
}
