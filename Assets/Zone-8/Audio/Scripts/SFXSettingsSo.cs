using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Zone8.Audio
{

    [CreateAssetMenu(menuName = "Zone8/Audio/SFX Settings")]
    [InlineEditor]
    public class SFXSettingsSo : SerializedScriptableObject
    {
        public AudioMixer TargetAudioMixer;

        public List<ETrack> Tracks;

        private const float k_mixerValuesMultiplier = 20;
        private const float k_minimalVolume = 0.0001f;



        public virtual void SetTrackVolume(ETrack track, float volume)
        {
            if (volume <= 0f)
            {
                volume = k_minimalVolume;
            }

            if (!Tracks.Contains(track))
            {
                Debug.LogError($"Track {track} not found in SFXSettingsSo");
                return;
            }

            if (!string.IsNullOrEmpty(track.Parameter))
                TargetAudioMixer.SetFloat(track.Parameter, NormalizedToMixerVolume(volume));
        }

        public virtual float GetTrackVolume(ETrack track)
        {
            float volume = 1f;

            if (!Tracks.Contains(track))
            {
                Debug.LogError($"Track {track} not found in SFXSettingsSo");
                return volume;
            }

            if (!string.IsNullOrEmpty(track.Parameter))
                TargetAudioMixer.GetFloat(track.Parameter, out volume);

            return MixerVolumeToNormalized(volume);
        }

        public virtual float NormalizedToMixerVolume(float normalizedVolume)
        {
            return Mathf.Log10(normalizedVolume) * k_mixerValuesMultiplier;
        }

        public virtual float MixerVolumeToNormalized(float mixerVolume)
        {
            return (float)Math.Pow(10, (mixerVolume / k_mixerValuesMultiplier));
        }

        #region Testing
        [Serializable]
        public class TrackTest
        {
            [HorizontalGroup("Track")]
            [HideLabel, ReadOnly, LabelWidth(100)]
            public ETrack Track;

            [HorizontalGroup("Track")]
            [Range(0f, 1f)]
            [OnValueChanged(nameof(OnVolumeChanged))]
            public float Volume;

            private SFXSettingsSo settings;

            public TrackTest(SFXSettingsSo settings, ETrack track)
            {
                this.settings = settings;
                Track = track;
                Volume = settings.GetTrackVolume(track);
            }

            private void OnVolumeChanged()
            {
                if (settings != null && Track != null)
                {
                    settings.SetTrackVolume(Track, Volume);
                }
            }
        }

        [TitleGroup("Testing", "Tracks", TitleAlignments.Centered, HorizontalLine = true)]
        [TableList]
        [ShowInInspector]
        private List<TrackTest> trackTests;

        [Button("Initialize Track Tests")]
        private void InitializeTrackTests()
        {
            trackTests = new List<TrackTest>();
            foreach (var track in Tracks)
            {
                trackTests.Add(new TrackTest(this, track));
            }
        }
        #endregion
    }
}