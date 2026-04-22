using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Zone8.Tweening
{
    public interface ITweenAction
    {
        CoreTweenSettings CoreSettings { get; set; }
        Tween Act(GameObject target);
    }

    [Serializable]
    public struct CoreTweenSettings
    {
        [BoxGroup("Timing")]
        [HorizontalGroup("Timing/Main", LabelWidth = 60)]
        public float Duration;

        [BoxGroup("Timing")]
        [HorizontalGroup("Timing/Main", LabelWidth = 40)]
        public float Delay;

        [BoxGroup("Animation")]
        public bool CustomEase;

        [BoxGroup("Animation")]
        [ShowIf(nameof(CustomEase))]
        public AnimationCurve EaseCurve;

        [BoxGroup("Animation")]
        [HideIf(nameof(CustomEase))]
        public Ease Ease;

        [BoxGroup("Looping")]
        public bool Loop;

        [BoxGroup("Looping")]
        [ShowIf(nameof(Loop))]
        [HorizontalGroup("Looping/Settings", LabelWidth = 70)]
        public int LoopCount;

        [BoxGroup("Looping")]
        [ShowIf(nameof(Loop))]
        [HorizontalGroup("Looping/Settings", LabelWidth = 70)]
        public LoopType LoopType;


        [BoxGroup("Technical")]
        [HorizontalGroup("Technical/Flags")]
        public bool IsIndependentUpdate;

        [BoxGroup("Technical")]
        [HorizontalGroup("Technical/Flags")]
        public bool IsRelative;

        [BoxGroup("Technical")]
        [HorizontalGroup("Technical/Final")]
        public UpdateType UpdateType;

        [BoxGroup("Technical")]
        [HorizontalGroup("Technical/Final")]
        public bool AutoKill;

        public void Apply(Tween t)
        {
            t.SetDelay(Delay)
             .SetUpdate(UpdateType, IsIndependentUpdate)
             .SetAutoKill(AutoKill)
             .SetRelative(IsRelative);

            if (CustomEase) t.SetEase(EaseCurve);
            else t.SetEase(Ease);

            if (Loop) t.SetLoops(LoopCount, LoopType);
        }
    }
}
