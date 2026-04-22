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


        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        [SerializeField] EActionType _actionType;

        [BoxGroup("Material Settings")]
        [SerializeField] int _materialIndex;

        [BoxGroup("Material Settings")]
        [SerializeField] string _targetProperty;

        [BoxGroup("Material Settings")]
        [SerializeField] bool _isTMP;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(_actionType), EActionType.Color)]
        [SerializeField] Color _toColor;

        [BoxGroup("Material Settings")]
        [ShowIf("@_actionType == EActionType.Fade || _actionType == EActionType.Float")]
        [SerializeField] float _toValue;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(_actionType), EActionType.Gradient)]
        [SerializeField] Gradient _toGradient;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(_actionType), EActionType.Offset)]
        [SerializeField] Vector2 _toOffset;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(_actionType), EActionType.Tiling)]
        [SerializeField] Vector2 _toTiling;

        [BoxGroup("Material Settings")]
        [ShowIf(nameof(_actionType), EActionType.Vector)]
        [SerializeField] Vector4 _toVector;


        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Logger.LogError("Null target");
                return null;
            }

            Material material = null;
            if (_isTMP)
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

                material = renderer.materials[_materialIndex];
            }

            if (material == null)
            {
                Logger.LogError("Target material is null");
                return null;
            }

            Tween tween;

            switch (_actionType)
            {
                case EActionType.Color:
                    tween = material.DOColor(_toColor, _targetProperty, CoreSettings.Duration);
                    break;
                case EActionType.Fade:
                    tween = material.DOFade(_toValue, _targetProperty, CoreSettings.Duration);
                    break;
                case EActionType.Float:
                    tween = material.DOFloat(_toValue, _targetProperty, CoreSettings.Duration);
                    break;
                case EActionType.Gradient:
                    tween = material.DOGradientColor(_toGradient, _targetProperty, CoreSettings.Duration);
                    break;
                case EActionType.Offset:
                    tween = material.DOOffset(_toOffset, _targetProperty, CoreSettings.Duration);
                    break;
                case EActionType.Tiling:
                    tween = material.DOTiling(_toTiling, _targetProperty, CoreSettings.Duration);
                    break;
                case EActionType.Vector:
                    tween = material.DOVector(_toVector, _targetProperty, CoreSettings.Duration);
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
