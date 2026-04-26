using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace Zone8.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXEmitter : MonoBehaviour
    {
        private AudioSource _audioSource;
        private IObjectPool<SFXEmitter> _emitterPool;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isPoolReleased;

        public bool IsPaused { get; private set; }
        public SFXClipSo Clip { get; private set; }


        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        private void OnDestroy()
        {
            StopPlayingTask();
        }

        #region API
        public void Initialize(SFXClipSo clip, IObjectPool<SFXEmitter> emitterPool)
        {
            if (clip == null)
            {
                Debug.LogError("SFXClip data is null.");
                return;
            }

            this._emitterPool = emitterPool;
            _isPoolReleased = false;
            Clip = clip;
            SetupAudioSource(clip);
        }

        public void Play(Action onEnd = null)
        {
            StopPlayingTask();

            _cancellationTokenSource = new CancellationTokenSource();
            _audioSource.Play();

            _ = WaitForSoundToEnd(onEnd);

        }

        public void Stop()
        {
            if (_isPoolReleased) return;
            _isPoolReleased = true;
            _audioSource.Stop();
            StopPlayingTask();
            _emitterPool.Release(this);
        }

        public void Pause()
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Pause();
                IsPaused = true;
            }
        }

        public void Resume()
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.UnPause();
                IsPaused = false;
            }
        }
        #endregion

        #region Private Methods

        private void SetupAudioSource(SFXClipSo clip)
        {
            _audioSource.clip = GetRandomClip(clip);
            _audioSource.outputAudioMixerGroup = clip.ClipTrack.Track;
            _audioSource.loop = clip.Loop;

            _audioSource.mute = clip.Mute;
            _audioSource.bypassEffects = clip.BypassEffects;
            _audioSource.bypassListenerEffects = clip.BypassListenerEffects;
            _audioSource.bypassReverbZones = clip.BypassReverbZones;

            _audioSource.volume = clip.RandomizeVolume ? Randomize(clip.MinVolume, clip.MaxVolume) : clip.Volume;
            _audioSource.pitch = clip.RandomizePitch ? Randomize(clip.MinPitch, clip.MaxPitch) : clip.Pitch;

            _audioSource.panStereo = clip.PanStereo;
            _audioSource.spatialBlend = clip.SpatialBlend;
            _audioSource.reverbZoneMix = clip.ReverbZoneMix;
            _audioSource.dopplerLevel = clip.DopplerLevel;
            _audioSource.spread = clip.Spread;

            _audioSource.minDistance = clip.MinDistance;
            _audioSource.maxDistance = clip.MaxDistance;

            _audioSource.ignoreListenerVolume = clip.IgnoreListenerVolume;
            _audioSource.ignoreListenerPause = clip.IgnoreListenerPause;

            _audioSource.rolloffMode = clip.RolloffMode;
        }

        private void StopPlayingTask()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Awaitable WaitForSoundToEnd(Action onEnd)
        {
            try
            {
                var token = _cancellationTokenSource?.Token;

                while ((_audioSource.isPlaying || IsPaused) && token?.IsCancellationRequested == false)
                {
                    await Awaitable.EndOfFrameAsync();
                }

                if (token?.IsCancellationRequested == true)
                {
                    return;
                }

                if (!IsPaused && onEnd != null)
                {
                    onEnd?.Invoke();
                }
                Stop();

            }
            catch (OperationCanceledException e)
            {
                Logger.Log("Sound playback canceled." + e);
            }
        }

        private AudioClip GetRandomClip(SFXClipSo data)
        {
            if (data.Clips == null || data.Clips.Length == 0)
            {
                Debug.LogWarning("No clip found for SFX: " + data.name);
                return null;
            }

            return data.Clips[UnityEngine.Random.Range(0, data.Clips.Length)];
        }

        private float Randomize(float min = 0f, float max = 1f)
        {
            return UnityEngine.Random.Range(min, max);
        }
        #endregion


    }
}
