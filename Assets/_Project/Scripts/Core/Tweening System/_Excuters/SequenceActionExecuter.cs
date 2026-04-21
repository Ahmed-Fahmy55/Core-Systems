using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Zone8.Tweening
{
    [System.Serializable]
    public struct AppendData
    {
        [TableColumnWidth(200)]
        public GameObject target;

        [TableColumnWidth(200)]
        public TweenActionSO tweenAction;

        public Tween CreateTween()
        {
            return tweenAction?.Act(target);
        }
    }

    [System.Serializable]
    public struct InsertData
    {
        [TableColumnWidth(200)]
        public GameObject target;

        [TableColumnWidth(200)]
        public TweenActionSO Action;

        [TableColumnWidth(200)]
        public int Index;

        public Tween CreateTween()
        {
            return Action?.Act(target);
        }
    }

    public class SequenceActionExecuter : MonoBehaviour
    {
        [FoldoutGroup("Tween Settings")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        public List<AppendData> AppendTweens = new List<AppendData>();

        [FoldoutGroup("Tween Settings")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        public List<InsertData> InsertTweens = new List<InsertData>();

        #region CoreSettings

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool PlayOnEnable { get; set; } = false;

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool RestartOnPlay { get; set; } = true;

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

        [field: SerializeField, BoxGroup("Core Settings")]
        public bool IsRelative { get; set; }
        #endregion


        private Sequence _sequence;


        private void Awake()
        {
            Init();
        }

        private void OnEnable()
        {
            if (PlayOnEnable)
            {
                Play();
            }
        }

        [FoldoutGroup("Execution Controls")]
        [Button("Play Back")]
        [GUIColor(0.6f, 0.6f, 1f)]
        public Sequence PlayBack()
        {
            if (_sequence == null) return null;
            _sequence.PlayBackwards();
            return _sequence;
        }

        [FoldoutGroup("Execution Controls")]
        [Button("Play ")]
        [GUIColor(0.6f, 0.6f, 1f)]
        public Sequence Play()
        {
            if (_sequence == null)
            {
                Logger.LogError("Failed to initialize sequence.");
                return null;
            }

            if (RestartOnPlay)
            {
                _sequence.Restart();

            }
            else
            {
                _sequence.Play();
            }
            return _sequence;
        }


        private void Init()
        {
            _sequence = DOTween.Sequence();
            _sequence
                .SetUpdate(UpdateType)
                .SetAutoKill(AutoKill)
                .SetRelative(IsRelative);

            if (CustomEase)
            {
                _sequence.SetEase(EaseCurve);
            }
            else
            {
                _sequence.SetEase(Ease);
            }

            if (Loop) _sequence.SetLoops(LoopCount, LoopType);

            foreach (var tweenData in AppendTweens)
            {
                var tween = tweenData.CreateTween();
                if (tween != null)
                {
                    _sequence.Append(tween);
                }
            }

            foreach (var tweenData in InsertTweens)
            {
                var tween = tweenData.CreateTween();
                if (tween != null)
                {
                    _sequence.Insert(tweenData.Index, tween);
                }
            }
            _sequence.Pause();
        }
    }
}
