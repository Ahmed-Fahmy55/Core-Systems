using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Zone8.Tweening
{
    public class CanvasGroupTweenAction : ITweenAction
    {
        [field: SerializeField] public CoreTweenSettings CoreSettings { get; set; }

        /////////////////////////////////////////////////////

        [BoxGroup("Canvas Group Settings", Order = 1)]
        [SerializeField] private float _toValue;

        public Tween Act(GameObject target)
        {
            if (target == null)
            {
                Logger.LogError("Null target");
                return null;
            }

            if (target.TryGetComponent<CanvasGroup>(out var canvasGroup) == false)
            {
                Logger.LogError("Target does not have a CanvasGroup component");
                return null;
            }

            var tween = canvasGroup.DOFade(_toValue, CoreSettings.Duration);

            if (CoreSettings.IsFrom)
                tween.From();

            CoreSettings.Apply(tween);
            return tween;
        }
    }
}
