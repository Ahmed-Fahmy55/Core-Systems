using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Zone8.Events;

namespace Zone8.Audio
{
    public struct AudioPlayEvent : IEvent
    {
        public SFXClipSo Clip;
        public Vector3 Position;
        public Action OnEnd;
    }

    public struct AudioControlEvent : IEvent
    {
        public SFXClipSo Clip;
        public EAudioControl Control;
    }

    public struct AudioTrackEvent : IEvent
    {
        public ETrack Track;
        public ETrackMode TrackMode;
        public float Volume;

        public AudioTrackEvent(ETrack track, ETrackMode trackMode, float volume)
        {
            Track = track;
            TrackMode = trackMode;
            Volume = volume;
        }
    }

    public enum EAudioControl
    {
        Pause,
        Resume,
        Stop,
    }

    public enum ETrackMode
    {
        Mute, Unmute, SetVolume, Stop
    }

    public class SFXManager : MonoBehaviour
    {
        [SerializeField] private SFXSettingsSo _tracksSettings;
        [SerializeField] private SFXEmitter _soundEmitterPrefab;
        [SerializeField] private bool _collectionCheck = true;
        [SerializeField] private int _maxPoolSize = 10;
        [SerializeField] private int _maxFrequentSoundInstances = 30;

        private IObjectPool<SFXEmitter> _soundEmitterPool;
        private readonly Dictionary<ETrack, Dictionary<SFXClipSo, HashSet<SFXEmitter>>> _activeSounds = new();
        private readonly Dictionary<SFXClipSo, LinkedList<SFXEmitter>> _frequentSounds = new();

        private EventBinding<AudioPlayEvent> _audioPlayBinding;
        private EventBinding<AudioControlEvent> _audioControBinding;
        private EventBinding<AudioTrackEvent> _audioTrackBinding;



        #region Unity Methods
        private void Awake()
        {
            _audioPlayBinding = new EventBinding<AudioPlayEvent>(OnAudioPlayed);
            _audioControBinding = new EventBinding<AudioControlEvent>(OnAudioControl);
            _audioTrackBinding = new EventBinding<AudioTrackEvent>(OnControlTrack);

            InitializePool();

        }

        private void OnEnable()
        {
            EventBus<AudioPlayEvent>.Register(_audioPlayBinding);
            EventBus<AudioControlEvent>.Register(_audioControBinding);
            EventBus<AudioTrackEvent>.Register(_audioTrackBinding);
        }

        private void OnDisable()
        {
            EventBus<AudioPlayEvent>.Deregister(_audioPlayBinding);
            EventBus<AudioControlEvent>.Deregister(_audioControBinding);
            EventBus<AudioTrackEvent>.Deregister(_audioTrackBinding);
        }
        #endregion


        #region API
        public void Play(SFXClipSo clip, Vector3 position = new(), Action onEnd = null)
        {
            if (!CanPlaySound(clip))
            {
                return;
            }

            SFXEmitter soundEmitter = _soundEmitterPool.Get();

            soundEmitter.Initialize(clip, _soundEmitterPool);
            soundEmitter.transform.position = position;
            soundEmitter.transform.parent = transform;
            soundEmitter.Play(onEnd);

            // Add emitter to nested dictionary
            if (!_activeSounds.TryGetValue(clip.ClipTrack, out var clipMap))
            {
                clipMap = new Dictionary<SFXClipSo, HashSet<SFXEmitter>>();
                _activeSounds[clip.ClipTrack] = clipMap;
            }
            if (!clipMap.TryGetValue(clip, out var emitterSet))
            {
                emitterSet = new HashSet<SFXEmitter>();
                clipMap[clip] = emitterSet;
            }
            emitterSet.Add(soundEmitter);

            //  frequent sounds
            if (clip.FrequentSound)
            {
                if (!_frequentSounds.ContainsKey(clip))
                {
                    _frequentSounds[clip] = new LinkedList<SFXEmitter>();
                }
                _frequentSounds[clip].AddLast(soundEmitter);
            }
        }

        public void StopSound(SFXClipSo clip)
        {
            if (_activeSounds.TryGetValue(clip.ClipTrack, out var clipMap) &&
                clipMap.TryGetValue(clip, out var emitters))
            {
                var emittersCopy = emitters.ToList();
                foreach (var soundEmitter in emittersCopy)
                {
                    soundEmitter.Stop();
                }
            }
        }

        public void StopTrackSounds(ETrack track)
        {
            if (track == null)
            {
                Debug.LogWarning("Ambiance track is not defined.");
                return;
            }

            if (!_activeSounds.TryGetValue(track, out var clipMap))
            {
                Debug.LogWarning($"No active sound emitters found for track: {track.name}");
                return;
            }

            foreach (var clip in clipMap.Keys.ToList())
            {
                StopSound(clip);
            }
        }

        public void StopAll()
        {
            // Collect emitters first so dictionary modifications don't break iteration
            var emitters = new List<SFXEmitter>();

            foreach (var trackMap in _activeSounds.Values)
            {
                foreach (var emitterList in trackMap.Values)
                {
                    emitters.AddRange(emitterList);
                }
            }

            // Now stop them safely
            foreach (var emitter in emitters)
            {
                emitter.Stop();
            }
        }

        public void ControlTrack(ETrack track, ETrackMode trackMode, float volume = default)
        {
            switch (trackMode)
            {
                case ETrackMode.Mute:
                    if (_tracksSettings.IsTrackMuted(track)) return;
                    track.MutedVolume = _tracksSettings.GetTrackVolume(track);
                    _tracksSettings.SetTrackVolume(track, 0f);
                    break;
                case ETrackMode.Unmute:
                    if (!_tracksSettings.IsTrackMuted(track)) return;
                    _tracksSettings.SetTrackVolume(track, track.MutedVolume);
                    break;
                case ETrackMode.SetVolume:
                    _tracksSettings.SetTrackVolume(track, volume);
                    break;
            }
        }

        public void OnControlTrack(AudioTrackEvent data)
        {
            switch (data.TrackMode)
            {
                case ETrackMode.Mute:
                    if (_tracksSettings.IsTrackMuted(data.Track)) return;
                    data.Track.MutedVolume = _tracksSettings.GetTrackVolume(data.Track);
                    _tracksSettings.SetTrackVolume(data.Track, 0f);
                    break;
                case ETrackMode.Unmute:
                    if (!_tracksSettings.IsTrackMuted(data.Track)) return;
                    _tracksSettings.SetTrackVolume(data.Track, data.Track.MutedVolume);
                    break;
                case ETrackMode.SetVolume:
                    _tracksSettings.SetTrackVolume(data.Track, data.Volume);
                    break;
                case ETrackMode.Stop:
                    StopTrackSounds(data.Track);
                    break;
            }
        }

        public float GetTrackVolume(ETrack track)
        {
            return _tracksSettings.GetTrackVolume(track);
        }

        public bool CanPlaySound(SFXClipSo clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("No clip data found.");
                return false;
            }

            if (!clip.FrequentSound) return true;

            if (!_frequentSounds.TryGetValue(clip, out var emitters)) return true;

            if (emitters.Count >= _maxFrequentSoundInstances)
            {
                try
                {
                    emitters.First.Value.Stop();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to stop sound emitter: {ex.Message}");
                }
                return false;
            }

            return true;
        }

        #endregion

        #region Private Methods
        private void InitializePool()
        {
            _soundEmitterPool = new ObjectPool<SFXEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                _collectionCheck,
                _maxPoolSize);
        }

        private SFXEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(_soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        private void OnTakeFromPool(SFXEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
        }

        private void OnReturnedToPool(SFXEmitter soundEmitter)
        {
            if (_activeSounds.TryGetValue(soundEmitter.Clip.ClipTrack, out var clipMap) &&
                 clipMap.TryGetValue(soundEmitter.Clip, out var emitters))
            {
                emitters.Remove(soundEmitter);
                if (emitters.Count == 0)
                {
                    clipMap.Remove(soundEmitter.Clip);
                }
                if (clipMap.Count == 0)
                {
                    _activeSounds.Remove(soundEmitter.Clip.ClipTrack);
                }
            }

            if (soundEmitter.Clip.FrequentSound &&
                _frequentSounds.TryGetValue(soundEmitter.Clip, out var list))
            {
                list.Remove(soundEmitter);
                if (list.Count == 0)
                {
                    _frequentSounds.Remove(soundEmitter.Clip);
                }
            }

            soundEmitter.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(SFXEmitter soundEmitter)
        {
            Destroy(soundEmitter.gameObject);
        }
        #endregion

        #region Callbacks
        private void OnAudioPlayed(AudioPlayEvent data)
        {
            Play(data.Clip, data.Position, data.OnEnd);
        }

        private void OnAudioControl(AudioControlEvent data)
        {
            if (_activeSounds.TryGetValue(data.Clip.ClipTrack, out var clipMap) &&
                 clipMap.TryGetValue(data.Clip, out var soundEmitters))
            {
                var emittersToControl = new List<SFXEmitter>(soundEmitters);

                foreach (var soundEmitter in emittersToControl)
                {
                    switch (data.Control)
                    {
                        case EAudioControl.Pause:
                            soundEmitter.Pause();
                            break;

                        case EAudioControl.Resume:
                            soundEmitter.Resume();
                            break;

                        case EAudioControl.Stop:
                            StopSound(soundEmitter.Clip);
                            break;

                        default:
                            Debug.LogWarning($"Unhandled sound control: {data.Control}");
                            break;
                    }
                }

                if (data.Control == EAudioControl.Stop)
                {
                    clipMap.Remove(data.Clip);
                    if (clipMap.Count == 0)
                    {
                        _activeSounds.Remove(data.Clip.ClipTrack);
                    }

                    if (data.Clip.FrequentSound) _frequentSounds.Remove(data.Clip);
                }
            }
            else
            {
                Debug.LogWarning($"No active sound emitters found for clip: {data.Clip?.name}");
            }
        }

        #endregion
    }
}






