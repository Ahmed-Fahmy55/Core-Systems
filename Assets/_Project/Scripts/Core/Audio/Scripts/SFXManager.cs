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
        public EControlMode Control;
    }

    public struct TrackControlEvent : IEvent
    {
        public ETrack Track;
        public ETrackMode TrackMode;
        public float Volume;

        public TrackControlEvent(ETrack track, ETrackMode trackMode, float volume)
        {
            Track = track;
            TrackMode = trackMode;
            Volume = volume;
        }
    }

    public enum EControlMode
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
        [SerializeField] private int _defaultCapacity = 30;
        [SerializeField] private int _maxFrequentSoundInstances = 30;

        private IObjectPool<SFXEmitter> _soundEmitterPool;
        private readonly Dictionary<ETrack, Dictionary<SFXClipSo, HashSet<SFXEmitter>>> _activeSounds = new();
        private readonly Dictionary<SFXClipSo, LinkedList<SFXEmitter>> _frequentSounds = new();

        private EventBinding<AudioPlayEvent> _audioPlayBinding;
        private EventBinding<AudioControlEvent> _audioControBinding;
        private EventBinding<TrackControlEvent> _audioTrackBinding;



        #region Unity Methods
        private void Awake()
        {
            _audioPlayBinding = new EventBinding<AudioPlayEvent>(OnAudioPlayed);
            _audioControBinding = new EventBinding<AudioControlEvent>(OnAudioControl);
            _audioTrackBinding = new EventBinding<TrackControlEvent>(OnTrackControl);

            InitializePool();
            PreWarmPool(_defaultCapacity);
        }

        private void OnEnable()
        {
            EventBus<AudioPlayEvent>.Register(_audioPlayBinding);
            EventBus<AudioControlEvent>.Register(_audioControBinding);
            EventBus<TrackControlEvent>.Register(_audioTrackBinding);
        }

        private void OnDisable()
        {
            EventBus<AudioPlayEvent>.Deregister(_audioPlayBinding);
            EventBus<AudioControlEvent>.Deregister(_audioControBinding);
            EventBus<TrackControlEvent>.Deregister(_audioTrackBinding);
        }
        private void OnDestroy()
        {
            StopAll();

            _soundEmitterPool.Clear();
        }

        #endregion


        #region API
        public void Play(SFXClipSo clip, Vector3 position = new(), Action onEnd = null)
        {
            if (!CanPlaySound(clip))
            {
                return;
            }

            if (_activeSounds.TryGetValue(clip.ClipTrack, out var clipsDic) &&
                clipsDic.TryGetValue(clip, out var emitters))
            {
                var pausedEmitter = emitters.FirstOrDefault(e => e.IsPaused);

                if (pausedEmitter != null)
                {
                    pausedEmitter.transform.position = position;
                    pausedEmitter.Resume();
                    return;
                }
            }

            SFXEmitter soundEmitter = _soundEmitterPool.Get();
            soundEmitter.Initialize(clip, _soundEmitterPool);
            soundEmitter.transform.position = position;
            soundEmitter.transform.parent = transform;
            soundEmitter.Play(onEnd);

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

            if (clip.FrequentSound)
            {
                if (!_frequentSounds.ContainsKey(clip))
                {
                    _frequentSounds[clip] = new LinkedList<SFXEmitter>();
                }
                _frequentSounds[clip].AddLast(soundEmitter);
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

            var pooledClips = ListPool<SFXClipSo>.Get();
            pooledClips.AddRange(clipMap.Keys);

            foreach (var clip in pooledClips)
                StopSound(clip);

            ListPool<SFXClipSo>.Release(pooledClips);
        }

        public void StopSound(SFXClipSo clip)
        {
            if (clip == null) return;

            if (_activeSounds.TryGetValue(clip.ClipTrack, out var clipMap) &&
                clipMap.TryGetValue(clip, out var emitters))
            {
                var pooledList = ListPool<SFXEmitter>.Get();

                try
                {
                    pooledList.AddRange(emitters);
                    foreach (var soundEmitter in pooledList)
                    {
                        if (soundEmitter != null)
                        {
                            soundEmitter.Stop();
                        }
                    }
                }
                finally
                {
                    ListPool<SFXEmitter>.Release(pooledList);
                }
            }
        }

        public void StopAll()
        {
            var pooledList = ListPool<SFXEmitter>.Get();

            try
            {
                foreach (var trackMap in _activeSounds.Values)
                {
                    foreach (var emitterSet in trackMap.Values)
                    {
                        pooledList.AddRange(emitterSet);
                    }
                }

                foreach (var emitter in pooledList)
                {
                    if (emitter != null)
                    {
                        emitter.Stop();
                    }
                }
            }
            finally
            {
                ListPool<SFXEmitter>.Release(pooledList);
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
                case ETrackMode.Stop:
                    StopTrackSounds(track);
                    break;
            }
        }


        #endregion

        #region Private Methods

        private bool CanPlaySound(SFXClipSo clip)
        {
            if (clip == null)
            {
                Logger.LogWarning("No clip data found.");
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

        private void InitializePool()
        {
            _soundEmitterPool = new ObjectPool<SFXEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                _collectionCheck,
                defaultCapacity: _defaultCapacity);
        }

        private SFXEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(_soundEmitterPrefab, transform);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        private void OnTakeFromPool(SFXEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
        }

        private void OnReturnedToPool(SFXEmitter soundEmitter)
        {
            if (_activeSounds.Count == 0)
            {
                soundEmitter.gameObject.SetActive(false);
                return;
            }

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
            if (soundEmitter != null && soundEmitter.gameObject != null)
            {
                Destroy(soundEmitter.gameObject);
            }
        }

        private void PreWarmPool(int count)
        {
            var tempPreWarmList = ListPool<SFXEmitter>.Get();

            for (int i = 0; i < count; i++)
            {
                tempPreWarmList.Add(_soundEmitterPool.Get());
            }

            foreach (var emitter in tempPreWarmList)
            {
                _soundEmitterPool.Release(emitter);
            }

            ListPool<SFXEmitter>.Release(tempPreWarmList);
        }
        #endregion

        #region Callbacks

        private void OnTrackControl(TrackControlEvent data)
        {
            ControlTrack(data.Track, data.TrackMode, data.Volume);
        }

        private void OnAudioControl(AudioControlEvent data)
        {
            if (data.Clip == null) return;

            if (_activeSounds.TryGetValue(data.Clip.ClipTrack, out var clipMap) &&
                clipMap.TryGetValue(data.Clip, out var soundEmitters))
            {
                var tempEmitters = ListPool<SFXEmitter>.Get();
                tempEmitters.AddRange(soundEmitters);

                try
                {
                    foreach (var soundEmitter in tempEmitters)
                    {
                        if (soundEmitter == null) continue;

                        switch (data.Control)
                        {
                            case EControlMode.Pause:
                                soundEmitter.Pause();
                                break;

                            case EControlMode.Resume:
                                soundEmitter.Resume();
                                break;

                            case EControlMode.Stop:
                                soundEmitter.Stop();
                                break;
                        }
                    }
                }
                finally
                {
                    ListPool<SFXEmitter>.Release(tempEmitters);
                }

                if (data.Control == EControlMode.Stop)
                {
                    clipMap.Remove(data.Clip);
                    if (clipMap.Count == 0)
                    {
                        _activeSounds.Remove(data.Clip.ClipTrack);
                    }

                    if (data.Clip.FrequentSound)
                        _frequentSounds.Remove(data.Clip);
                }
            }
        }

        private void OnAudioPlayed(AudioPlayEvent data)
        {
            Play(data.Clip, data.Position, data.OnEnd);
        }

        #endregion
    }
}






