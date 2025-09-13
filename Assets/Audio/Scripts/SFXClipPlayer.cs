using Sirenix.OdinInspector;
using UnityEngine;

namespace Bltzo.Audio
{
    public class SFXClipPlayer : MonoBehaviour
    {
        [SerializeField] private SFXClip clip;
        [SerializeField] private bool _playOnStart = true;


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
            if (clip != null)
            {
                clip.Play();
            }
        }

        [Button]
        public void StopClip()
        {
            if (clip != null)
            {
                clip.Stop();
            }
        }
    }
}
