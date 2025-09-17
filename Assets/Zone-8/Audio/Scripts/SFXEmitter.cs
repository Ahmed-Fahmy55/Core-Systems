using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace Zone8.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXEmitter : MonoBehaviour
    {
        private AudioSource _audioSource;
        private Task _playingTask;
        private IObjectPool<SFXEmitter> _emitterPool;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isPaused = false;

        public SFXClip Clip { get; private set; }


        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        #region API
        public void Initialize(SFXClip clip, IObjectPool<SFXEmitter> emitterPool)
        {
            if (clip == null)
            {
                Debug.LogError("SFXClip data is null.");
                return;
            }

            this._emitterPool = emitterPool;
            Clip = clip;
            SetupAudioSource(clip);
        }



        public void Play(Action onEnd = null)
        {
            if (_playingTask != null && !_playingTask.IsCompleted)
            {
                StopPlayingTask();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _audioSource.Play();

            _playingTask = WaitForSoundToEnd(onEnd);

        }

        public void Stop()
        {
            _audioSource.Stop();
            StopPlayingTask();
            _emitterPool.Release(this);
        }

        public void Pause()
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Pause();
                _isPaused = true;
            }
        }

        public void Resume()
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.UnPause();
                _isPaused = false;
            }
        }
        #endregion

        #region Private Methods

        private void SetupAudioSource(SFXClip clip)
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

        private async Task WaitForSoundToEnd(Action onEnd)
        {
            try
            {
                var token = _cancellationTokenSource?.Token;

                while ((_audioSource.isPlaying || _isPaused) && token?.IsCancellationRequested == false)
                {
                    // Await the next frame to prevent blocking the main thread
                    await Task.Yield();
                }

                if (token?.IsCancellationRequested == true)
                {
                    return;
                }

                if (!_isPaused && onEnd != null)
                {
                    onEnd?.Invoke();
                }
                Stop();

            }
            catch (OperationCanceledException)
            {
                Debug.Log("Sound playback canceled.");
            }
        }

        private AudioClip GetRandomClip(SFXClip data)
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
