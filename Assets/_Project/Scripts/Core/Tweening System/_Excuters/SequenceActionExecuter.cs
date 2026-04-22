using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zone8.Tweening
{

    public enum SequenceMode { Append, Join }

    [Serializable]
    public struct SequenceData
    {
        [EnumToggleButtons, HideLabel]
        public SequenceMode Mode;
        public GameObject Target;
        public TweenActionSO Action;

    }

    [Serializable]
    public struct InsertData
    {
        [Tooltip("The absolute time (in seconds) from the start of the sequence")]
        public float StartTime;
        public GameObject Target;
        public TweenActionSO Action;
    }

    public class SequenceActionExecuter : MonoBehaviour
    {
        [ListDrawerSettings(ShowIndexLabels = true, ElementColor = "GetElementColor")]
        public List<SequenceData> SequenceTweens = new List<SequenceData>();

        [ListDrawerSettings(ShowIndexLabels = true)]
        public List<InsertData> InsertTweens = new List<InsertData>();

        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private bool _restartOnPlay = true;
        [SerializeField] bool _overrideSequenceSettings = false;

        [HideLabel, ShowIf(nameof(_overrideSequenceSettings))]
        [SerializeField] CoreTweenSettings sequenceSettings;

        public Sequence Sequence { get; private set; }

        private void Start()
        {
            BuildSequence();
            if (_playOnStart) Play();
        }

        [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void Play()
        {
            if (Sequence == null || !Sequence.IsActive()) BuildSequence();

            if (_restartOnPlay) Sequence.Restart();
            else Sequence.Play();

        }

        [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void PlayBack()
        {
            if (Sequence == null || !Sequence.IsActive()) BuildSequence();
            Sequence.Complete();
            Sequence.PlayBackwards();
        }

        [Button, GUIColor(1, 0.5f, 0)]
        public void BuildSequence()
        {
            if (Sequence != null) Sequence.Kill();

            Sequence = DOTween.Sequence();

            sequenceSettings.Apply(Sequence);

            foreach (var data in SequenceTweens)
            {
                if (data.Action == null || data.Target == null)
                {
                    Logger.LogWarning("Null tween action or target in AppendTweens, skipping entry.");
                    continue;
                }

                Tween t = data.Action.Act(data.Target);

                if (data.Mode == SequenceMode.Join)
                    Sequence.Join(t);
                else
                    Sequence.Append(t);
            }

            foreach (var data in InsertTweens)
            {
                if (data.Action == null || data.Target == null)
                {
                    Logger.LogWarning("Null tween action or target in Insert tweens, skipping entry.");
                    continue;
                }
                Sequence.Insert(data.StartTime, data.Action.Act(data.Target));
            }

            Sequence.Pause();
        }

        private Color GetElementColor(int index, Color defaultColor)
        {
            if (index < SequenceTweens.Count && SequenceTweens[index].Mode == SequenceMode.Join)
                return new Color(0.2f, 0.5f, 1f, 0.2f); // Blue tint for Joined items
            return defaultColor;
        }

        private void OnDestroy()
        {
            Sequence?.Kill();
        }

    }
}
