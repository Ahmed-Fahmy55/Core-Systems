using DG.Tweening;
using UnityEngine;

namespace Zone8.Tweening
{
    public interface ITweenAction
    {
        float Duration { get; set; }
        float Delay { get; set; }
        bool Loop { get; set; }
        int LoopCount { get; set; }
        LoopType LoopType { get; set; }
        bool CustomEase { get; set; }
        AnimationCurve EaseCurve { get; set; }
        Ease Ease { get; set; }
        UpdateType UpdateType { get; set; }
        bool AutoKill { get; set; }

        Tween Act(GameObject target);
    }
}
