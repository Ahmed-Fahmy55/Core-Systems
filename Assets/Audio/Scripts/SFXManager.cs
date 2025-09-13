using Bltzo.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Bltzo.Audio
{
    public struct AudioPlayEvent : IEvent
    {
        public SFXClip Clip;
        public Vector3 Position;
        public Action OnEnd;
    }

    public struct AudioControlEvent : IEvent
    {
        public SFXClip Clip;
        public EAudioControl Control;
    }

    public enum EAudioControl
    {
        Pause,
        Resume,
        Stop,
    }

    public enum ETrackMode
    {
        Mute, Unmute, SetVolume
    }

    public class SFXManager : MonoBehaviour
    {
        [SerializeField] private SFXSettingsSo _tracksSettings;
        [SerializeField] private SFXEmitter _soundEmitterPrefab;
        [SerializeField] private bool _collectionCheck = true;
        [SerializeField] private int _maxPoolSize = 10;
        [SerializeField] private int _maxFrequentSoundInstances = 30;

        private IObjectPool<SFXEmitter> _soundEmitterPool;
        private readonly Dictionary<SFXClip, HashSet<SFXEmitter>> _activeSoundEmittersDic = new();
        private readonly Dictionary<SFXClip, LinkedList<SFXEmitter>> _frequentSoundEmittersDic = new();

        private EventBinding<AudioPlayEvent> _audioPlayedBinding;
        private EventBinding<AudioControlEvent> _audioControldBinding;


        #region Unity Methods
        private void Awake()
        {
            _audioPlayedBinding = new EventBinding<AudioPlayEvent>(OnAudioPlayed);
            _audioControldBinding = new EventBinding<AudioControlEvent>(OnAudioControl);
            InitializePool();

        }

        private void OnEnable()
        {
            EventBus<AudioPlayEvent>.Register(_audioPlayedBinding);
            EventBus<AudioControlEvent>.Register(_audioControldBinding);
        }

        private void OnDisable()
        {
            EventBus<AudioPlayEvent>.Deregister(_audioPlayedBinding);
            EventBus<AudioControlEvent>.Deregister(_audioControldBinding);
        }
        #endregion


        #region API
        public void Play(SFXClip clip, Vector3 position = new(), Action onEnd = null)
        {

            if (!CanPlaySound(clip))
            {
                return;
            }

            SFXEmitter soundEmitter = Get();

            soundEmitter.Initialize(clip, _soundEmitterPool);
            soundEmitter.transform.position = position;
            soundEmitter.transform.parent = transform;
            soundEmitter.Play(onEnd);

            if (!_activeSoundEmittersDic.ContainsKey(clip))
            {
                _activeSoundEmittersDic[clip] = new HashSet<SFXEmitter>();
            }
            _activeSoundEmittersDic[clip].Add(soundEmitter);

            if (!clip.FrequentSound) return;

            if (!_frequentSoundEmittersDic.ContainsKey(clip))
            {
                _frequentSoundEmittersDic[clip] = new LinkedList<SFXEmitter>();
            }
            _frequentSoundEmittersDic[clip].AddLast(soundEmitter);
        }

        public void StopSound(SFXClip clip)
        {
            if (_activeSoundEmittersDic.TryGetValue(clip, out var soundEmitters))
            {
                var emittersCopy = soundEmitters.ToList();

                foreach (var soundEmitter in emittersCopy)
                {
                    soundEmitter.Stop();
                }

                _activeSoundEmittersDic.Remove(clip);

                if (clip.FrequentSound) _frequentSoundEmittersDic.Remove(clip);
            }
        }

        public void StopAll()
        {
            var clips = _activeSoundEmittersDic.Keys.ToList();

            foreach (var clip in clips)
            {
                StopSound(clip);
            }

            _activeSoundEmittersDic.Clear();
            _frequentSoundEmittersDic.Clear();
        }

        public void ControlTrack(ETrack track, ETrackMode trackMode, float volume = 0.5f)
        {
            switch (trackMode)
            {
                case ETrackMode.Mute:
                    track.MutedVolume = _tracksSettings.GetTrackVolume(track);
                    _tracksSettings.SetTrackVolume(track, 0f);
                    break;
                case ETrackMode.Unmute:
                    _tracksSettings.SetTrackVolume(track, track.MutedVolume);
                    break;
                case ETrackMode.SetVolume:
                    _tracksSettings.SetTrackVolume(track, volume);
                    break;
            }
        }
        public bool CanPlaySound(SFXClip clip)
        {
            if (clip == null)
            {
                Debug.LogWarning("No clip data found.");
                return false;
            }

            if (!clip.FrequentSound) return true;

            if (!_frequentSoundEmittersDic.TryGetValue(clip, out var emitters)) return true;

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
        private SFXEmitter Get()
        {
            return _soundEmitterPool.Get();
        }

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
            // Deactivate the emitter
            if (_activeSoundEmittersDic.TryGetValue(soundEmitter.Clip, out var soundEmitters))
            {
                soundEmitters.Remove(soundEmitter);
                if (soundEmitters.Count == 0)
                {
                    _activeSoundEmittersDic.Remove(soundEmitter.Clip);
                }
            }

            if (soundEmitter.Clip.FrequentSound)
            {
                if (_frequentSoundEmittersDic.TryGetValue(soundEmitter.Clip, out var emitters))
                {
                    emitters.Remove(soundEmitter);
                    if (emitters.Count == 0)
                    {
                        _frequentSoundEmittersDic.Remove(soundEmitter.Clip);
                    }
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

        private void OnAudioControl(AudioControlEvent @event)
        {
            if (!_activeSoundEmittersDic.TryGetValue(@event.Clip, out var soundEmitters))
            {
                Debug.LogWarning($"No active sound emitters found for clip: {@event.Clip.name}");
                return;
            }

            // Create a temporary list to avoid modifying the collection during iteration
            var emittersToControl = new List<SFXEmitter>(soundEmitters);

            foreach (var soundEmitter in emittersToControl)
            {
                switch (@event.Control)
                {
                    case EAudioControl.Pause:
                        soundEmitter.Pause();
                        break;

                    case EAudioControl.Resume:
                        soundEmitter.Resume();
                        break;

                    case EAudioControl.Stop:
                        soundEmitter.Stop();
                        break;

                    default:
                        Debug.LogWarning($"Unhandled sound control: {@event.Control}");
                        break;
                }
            }

            // If the control is Stop, remove the clip from the active dictionary
            if (@event.Control == EAudioControl.Stop)
            {
                _activeSoundEmittersDic.Remove(@event.Clip);
                if (@event.Clip.FrequentSound) _frequentSoundEmittersDic.Remove(@event.Clip);
            }
        }
        #endregion
    }
}





