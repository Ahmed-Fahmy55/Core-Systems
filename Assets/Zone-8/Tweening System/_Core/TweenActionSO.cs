using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Tweening
{
    [InlineEditor]
    [CreateAssetMenu(menuName = "Zone8/Tweening/Tween Action")]
    public class TweenActionSO : SerializedScriptableObject
    {
        [SerializeField] protected ITweenAction tweenAction;

        public ITweenAction Tween => tweenAction;
        public Tween Act(GameObject target)
        {
            if (tweenAction == null)
            {
                Logger.LogError("Null tween action");
                return null;
            }
            if (target == null)
            {
                Logger.LogError("Null target");
                return null;
            }

            return tweenAction.Act(target);
        }

    }

    public enum ETransformType
    {
        Position, Rotation, Scale
    }

    public enum EActionType
    {
        Movement, Rotation, Scale, Path, Punch, Shake
    }
}
