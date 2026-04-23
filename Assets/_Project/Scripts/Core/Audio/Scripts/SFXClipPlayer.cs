using Sirenix.OdinInspector;
using UnityEngine;
using Zone8.Events;

namespace Zone8.Audio
{
    public class SFXClipPlayer : MonoBehaviour
    {
        [SerializeField] private SFXClipSo _clip;
        [SerializeField] private bool _playOnStart = true;
        [SerializeField] private bool _stopTrackBeforePlay;


        private void Start()
        {
            if (_playOnStart)
            {
                PlayClip();
            }
        }

        [Button]
        public void PlayClip()
        {
            if (_clip == null) return;

            if (_stopTrackBeforePlay)
            {
                EventBus<TrackControlEvent>.Raise(new TrackControlEvent
                {
                    Track = _clip.ClipTrack,
                    TrackMode = ETrackMode.Stop
                });
            }

            _clip.Play();

        }

        [Button]
        public void StopClip()
        {
            if (_clip != null)
            {
                _clip.Stop();
            }
        }
    }
}
