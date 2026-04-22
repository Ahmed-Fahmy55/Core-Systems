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

        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [BoxGroup("Image Settings", Order = 1)]
        [SerializeField] EActionType _actionType;

        [BoxGroup("Image Settings", Order = 1), ShowIf(nameof(_actionType), EActionType.Color)]
        [SerializeField] Color _toColor;

        [BoxGroup("Image Settings", Order = 1), ShowIf("@_actionType == EActionType.Fade || _actionType == EActionType.Fill")]
        [SerializeField] float _toValue;

        [BoxGroup("Image Settings", Order = 1), ShowIf(nameof(_actionType), EActionType.GradientColor)]
        [InfoBox("Changes the target's color via the given gradient.\r\nNOTE: Only uses the colors of the gradient, not the alphas.\r\nNOTE: Creates a Sequence, not a Tweener.")]
        [SerializeField] Gradient _toGradient;


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

            switch (_actionType)
            {
                case EActionType.Color:
                    tween = image.DOColor(_toColor, CoreSettings.Duration);
                    break;
                case EActionType.Fade:
                    tween = image.DOFade(_toValue, CoreSettings.Duration);
                    break;
                case EActionType.Fill:
                    tween = image.DOFillAmount(_toValue, CoreSettings.Duration);
                    break;
                case EActionType.GradientColor:
                    tween = image.DOGradientColor(_toGradient, CoreSettings.Duration);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                    nameof(_actionType),
                    _actionType,
                    "Unhandled EActionType value"
               );
            }

            CoreSettings.Apply(tween);

            return tween;
        }
    }
}
