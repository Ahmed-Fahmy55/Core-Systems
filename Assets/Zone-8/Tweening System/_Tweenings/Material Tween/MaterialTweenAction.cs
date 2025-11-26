using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

namespace Zone8.Tweening
{
    public class MaterialTweenAction : ITweenAction
    {
        enum EActionType
        {
            Color, Fade, Float, Gradient, Offset, Tiling, Vector
        }

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
        #endregion }

        /////////////////////////////////////////////////////
        [BoxGroup("Material Settings")]
        [SerializeField] int materialIndex;
        [SerializeField] EActionType actionType;

        [BoxGroup("Material Settings")]
        [SerializeField] string targetProperty;

        [BoxGroup("Material Settings")]
        [SerializeField] bool isTMP;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(actionType), EActionType.Color)]
        [SerializeField] Color toColor;

        [BoxGroup("Material Settings")]
        [ShowIf("@actionType == EActionType.Fade || actionType == EActionType.Float")]
        [SerializeField] float toValue;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(actionType), EActionType.Gradient)]
        [SerializeField] Gradient toGradient;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(actionType), EActionType.Offset)]
        [SerializeField] Vector2 toOffset;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(actionType), EActionType.Tiling)]
        [SerializeField] Vector2 toTiling;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(actionType), EActionType.Vector)]
        [SerializeField] Vector4 toVector;

        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Logger.LogError("Null target");
                return null;
            }

            Material material = null;
            if (isTMP)
            {
                if (target.TryGetComponent<TextMeshProUGUI>(out var text) == false)
                {
                    Logger.LogError("Target does not have a Renderer component");
                    return null;
                }
                material = text.fontMaterial;
            }
            else
            {
                if (target.TryGetComponent<Renderer>(out var renderer) == false)
                {
                    Logger.LogError("Target does not have a Renderer component");
                    return null;
                }

                material = renderer.materials[materialIndex];
            }

            if (material == null)
            {
                Logger.LogError("Target material is null");
                return null;
            }

            Tween tween;

            switch (actionType)
            {
                case EActionType.Color:
                    tween = material.DOColor(toColor, targetProperty, Duration);
                    break;
                case EActionType.Fade:
                    tween = material.DOFade(toValue, targetProperty, Duration);
                    break;
                case EActionType.Float:
                    tween = material.DOFloat(toValue, targetProperty, Duration);
                    break;
                case EActionType.Gradient:
                    tween = material.DOGradientColor(toGradient, targetProperty, Duration);
                    break;
                case EActionType.Offset:
                    tween = material.DOOffset(toOffset, targetProperty, Duration);
                    break;
                case EActionType.Tiling:
                    tween = material.DOTiling(toTiling, targetProperty, Duration);
                    break;
                case EActionType.Vector:
                    tween = material.DOVector(toVector, targetProperty, Duration);
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
