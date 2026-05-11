using UnityEngine;
using UnityEngine.UI;
using Zone8.Audio.Data;
using Zone8.Events;

namespace Zone8.Audio.Components
{
    public class TrackSliderUI : MonoBehaviour
    {
        [SerializeField] private ETrack _track;
        [SerializeField] private Slider _slider;

        private TrackControlEvent _audioTrackEvent;
        private SFXManager _sfxManager;

        private void Awake()
        {
            _sfxManager = FindAnyObjectByType<SFXManager>();
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            _audioTrackEvent = new TrackControlEvent(_track, ETrackMode.SetVolume, 1);
        }

        private void Start()
        {
            _slider.value = _sfxManager.TracksSettings.GetTrackVolume(_track);
        }

        private void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            _audioTrackEvent.Volume = value;
            EventBus<TrackControlEvent>.Raise(_audioTrackEvent);
        }
    }
}
