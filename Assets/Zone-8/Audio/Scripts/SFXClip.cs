using Bltzo.Events;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Bltzo.Audio
{
    [CreateAssetMenu(menuName = "Bltzo/Audio/SFX Clip")]
    public class SFXClip : ScriptableObject
    {
        [TitleGroup("Audio Clips", "Audio Settings", TitleAlignments.Centered, HorizontalLine = true)]
        [Tooltip("List of audio clips to play.")]
        [GUIColor(0.8f, 0.9f, 1f)] // Light blue color
        public AudioClip[] Clips;

        [TitleGroup("Audio Mixer", "Audio Mixer Settings", TitleAlignments.Centered, HorizontalLine = true)]
        [Tooltip("Audio mixer group for this clip.")]
        [GUIColor(0.9f, 0.8f, 1f)] // Light purple color
        public AudioMixerGroup MixerGroup;

        [TitleGroup("Playback Settings", "Playback Options", TitleAlignments.Centered, HorizontalLine = true)]
        [HorizontalGroup("Playback Settings/Row1", Width = 0.5f)]
        [GUIColor(0.8f, 1f, 0.8f)] // Light green color
        public bool Loop;

        [HorizontalGroup("Playback Settings/Row1", Width = 0.5f), LabelWidth(100)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public bool PlayOnAwake;

        [HorizontalGroup("Playback Settings/Row2", Width = 0.5f), LabelWidth(100)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public bool FrequentSound;

        [HorizontalGroup("Playback Settings/Row2", Width = 0.5f)]
        [GUIColor(1f, 0.8f, 0.8f)] // Light red color
        public bool Mute;

        [TitleGroup("Volume Settings", "Volume Controls", TitleAlignments.Centered, HorizontalLine = true)]
        [HorizontalGroup("Volume Settings/Row1", Width = 0.5f)]
        [Range(0f, 1f)]
        [GUIColor(0.8f, 0.9f, 1f)]
        public float Volume = 1f;

        [HorizontalGroup("Volume Settings/Row1", Width = 0.5f), LabelWidth(150)]
        [GUIColor(0.8f, 0.9f, 1f)]
        public bool RandomizeVolume;

        [HorizontalGroup("Volume Settings/Row2", Width = 0.5f)]
        [ShowIf(nameof(RandomizeVolume))]
        [Range(0f, 1f)]
        [GUIColor(0.8f, 0.9f, 1f)]
        public float MinVolume;

        [HorizontalGroup("Volume Settings/Row2", Width = 0.5f)]
        [ShowIf(nameof(RandomizeVolume))]
        [Range(0f, 1f)]
        [GUIColor(0.8f, 0.9f, 1f)]
        public float MaxVolume;

        [TitleGroup("Pitch Settings", "Pitch Controls", TitleAlignments.Centered, HorizontalLine = true)]
        [HorizontalGroup("Pitch Settings/Row1", Width = 0.5f)]
        [Range(0.1f, 3f)]
        [GUIColor(0.9f, 0.8f, 1f)]
        public float Pitch = 1f;

        [HorizontalGroup("Pitch Settings/Row1", Width = 0.5f), LabelWidth(150)]
        [GUIColor(0.9f, 0.8f, 1f)]
        public bool RandomizePitch;

        [HorizontalGroup("Pitch Settings/Row2", Width = 0.5f)]
        [ShowIf(nameof(RandomizePitch))]
        [Range(-3f, 0f)]
        [GUIColor(0.9f, 0.8f, 1f)]
        public float MinPitch;

        [HorizontalGroup("Pitch Settings/Row2", Width = 0.5f),]
        [ShowIf(nameof(RandomizePitch))]
        [Range(-3f, 0f)]
        [GUIColor(0.9f, 0.8f, 1f)]
        public float MaxPitch;

        [TitleGroup("3D Sound Settings", "Spatial Controls", TitleAlignments.Centered, HorizontalLine = true)]
        [HorizontalGroup("3D Sound Settings/Row1", Width = 0.5f)]
        [Range(-1f, 1f)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public float PanStereo;

        [HorizontalGroup("3D Sound Settings/Row1", Width = 0.5f), LabelWidth(100)]
        [Range(0f, 1f)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public float SpatialBlend;

        [HorizontalGroup("3D Sound Settings/Row2", Width = 0.5f)]
        [Range(0f, 1f)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public float ReverbZoneMix = 1f;

        [HorizontalGroup("3D Sound Settings/Row2", Width = 0.5f), LabelWidth(100)]
        [Range(0f, 5f)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public float DopplerLevel = 1f;

        [HorizontalGroup("3D Sound Settings/Row3", Width = 0.5f)]
        [Range(0f, 360f)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public float Spread;


        [HorizontalGroup("3D Sound Settings/Row3", Width = 0.5f)]
        [GUIColor(0.9f, 0.8f, 1f)]
        public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;

        [TitleGroup("Distance Settings", "Distance Controls", TitleAlignments.Centered, HorizontalLine = true)]
        [HorizontalGroup("Distance Settings/Row1", Width = 0.5f), LabelWidth(100)]
        [MinValue(0.1f)]
        [GUIColor(1f, 0.8f, 0.8f)]
        public float MinDistance = 1f;

        [HorizontalGroup("Distance Settings/Row1", Width = 0.5f), LabelWidth(100)]
        [MinValue(0.1f)]
        [GUIColor(1f, 0.8f, 0.8f)]
        public float MaxDistance = 500f;

        [TitleGroup("Listener Settings", "Listener Controls", TitleAlignments.Centered, HorizontalLine = true)]
        [HorizontalGroup("Listener Settings/Row1", Width = 0.5f), LabelWidth(150)]
        [GUIColor(1f, 1f, 0.8f)]
        public bool IgnoreListenerVolume;

        [HorizontalGroup("Listener Settings/Row1", Width = 0.5f), LabelWidth(150)]
        [GUIColor(1f, 1f, 0.8f)]
        public bool IgnoreListenerPause;

        [TitleGroup("Bypass Settings", "Bypass Options", TitleAlignments.Centered, HorizontalLine = true)]
        [VerticalGroup("Bypass Settings/Column")]
        [GUIColor(1f, 1f, 0.8f)] // Light yellow color
        public bool BypassEffects;

        [VerticalGroup("Bypass Settings/Column")]
        [GUIColor(1f, 1f, 0.8f)]
        public bool BypassListenerEffects;

        [VerticalGroup("Bypass Settings/Column")]
        [GUIColor(1f, 1f, 0.8f)]
        public bool BypassReverbZones;

        public void Play(Vector3 position = new(), Action onEnd = null)
        {
            EventBus<AudioPlayEvent>.Raise(new AudioPlayEvent
            {
                Clip = this,
                Position = position,
                OnEnd = onEnd
            });
        }

        [TitleGroup("Testing", "", TitleAlignments.Centered, HorizontalLine = true)]
        [HorizontalGroup("Testing/Buttons", Width = 0.25f)] // Adjust width to fit all buttons
        [Button("Play")]
        [GUIColor(0.8f, 1f, 0.8f)]
        private void Play()
        {
            EventBus<AudioPlayEvent>.Raise(new AudioPlayEvent
            {
                Clip = this,
                Position = new(),
                OnEnd = null
            });
        }

        [HorizontalGroup("Testing/Buttons", Width = 0.25f)]
        [Button("Stop")]
        [GUIColor(1f, 0.8f, 0.8f)]
        public void Stop()
        {
            EventBus<AudioControlEvent>.Raise(new AudioControlEvent
            {
                Clip = this,
                Control = EAudioControl.Stop
            });
        }

        [HorizontalGroup("Testing/Buttons", Width = 0.25f)]
        [Button("Pause")]
        [GUIColor(1f, 1f, 0.8f)] // Light yellow color
        public void Pause()
        {
            EventBus<AudioControlEvent>.Raise(new AudioControlEvent
            {
                Clip = this,
                Control = EAudioControl.Pause
            });
        }

        [HorizontalGroup("Testing/Buttons", Width = 0.25f)]
        [Button("Resume")]
        [GUIColor(0.8f, 1f, 1f)] // Light cyan color
        public void Resume()
        {
            EventBus<AudioControlEvent>.Raise(new AudioControlEvent
            {
                Clip = this,
                Control = EAudioControl.Resume
            });
        }
    }
}
