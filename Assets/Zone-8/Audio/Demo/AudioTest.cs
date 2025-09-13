using Sirenix.OdinInspector;
using UnityEngine;
using Zone8.Audio;

public class AudioTest : MonoBehaviour
{
    [SerializeField] private SFXClip _sfxClip;
    [SerializeField] ETrack _track;
    [SerializeField] float _trackVolume = 0.5f;
    [SerializeField] private int _playtime = 5;

    private SFXManager sfxManager;

    private void Awake()
    {
        sfxManager = FindAnyObjectByType<SFXManager>();
    }

    [Button, HorizontalGroup("AudioControls")]
    void Play()
    {
        for (int i = 0; i < _playtime; i++)
        {
            _sfxClip.Play();
        }
    }

    [Button, HorizontalGroup("AudioControls")]
    void Pause()
    {
        _sfxClip.Pause();
    }

    [Button, HorizontalGroup("AudioControls")]
    void Resume()
    {
        _sfxClip.Resume();
    }

    [Button, HorizontalGroup("AudioControls")]
    void Stop()
    {
        _sfxClip.Stop();
    }

    [Button, HorizontalGroup("AudioControls")]
    void StopAll()
    {
        sfxManager.StopAll();
    }

    [Button, HorizontalGroup("TrackControls")]
    void MuteTrack()
    {
        sfxManager.ControlTrack(_track, ETrackMode.Mute);
    }

    [Button, HorizontalGroup("TrackControls")]
    void UnmuteTrack()
    {
        sfxManager.ControlTrack(_track, ETrackMode.Unmute);
    }

    [Button, HorizontalGroup("TrackControls")]
    void SetTrackVolume()
    {
        sfxManager.ControlTrack(_track, ETrackMode.SetVolume, _trackVolume);
    }

    [Button, HorizontalGroup("TrackControls")]
    void StopTrackSounds()
    {
        sfxManager.StopTrackSounds(_track);
    }
}
