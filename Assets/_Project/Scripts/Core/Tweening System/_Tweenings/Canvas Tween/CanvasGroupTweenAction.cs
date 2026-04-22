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
        [SerializeField] private float _endValue;

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

            Tween tween;
            tween = canvasGroup.DOFade(_endValue, CoreSettings.Duration);
            CoreSettings.Apply(tween);
            return tween;
        }
    }
}
