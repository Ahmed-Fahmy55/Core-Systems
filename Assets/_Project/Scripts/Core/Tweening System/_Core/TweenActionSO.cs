using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Tweening
{
    [InlineEditor]
    [CreateAssetMenu(menuName = "Tweening/Tween Action")]
    public class TweenActionSO : SerializedScriptableObject
    {
        [SerializeField] protected ITweenAction _tweenAction;

        public ITweenAction Tween => _tweenAction;

        public Tween Act(GameObject target)
        {
            if (_tweenAction == null)
            {
                Logger.LogError("Null tween action");
                return null;
            }
            if (target == null)
            {
                Logger.LogError("Null Tween target");
                return null;
            }

            return _tweenAction.Act(target);
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
