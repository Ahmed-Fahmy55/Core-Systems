using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Tweening
{
    public class TweenActionExecuter : MonoBehaviour
    {
        [SerializeField] GameObject _target;
        [SerializeField] protected bool playOnStart = true;
        [SerializeField] protected TweenActionSO action;

        Tween _tween;

        private void Start()
        {
            if (_target == null) _target = gameObject;

            if (playOnStart) _tween = Act();
        }

        [Button]
        public Tween Act()
        {
            return _tween = action?.Act(gameObject);
        }

        [Button]
        public void PlayBack()
        {
            _tween?.PlayBackwards();
        }

        [Button]
        public void Restart()
        {
            _tween?.Restart();
        }
    }
}
