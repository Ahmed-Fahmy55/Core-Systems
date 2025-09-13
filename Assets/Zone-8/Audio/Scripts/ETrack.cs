using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace Zone8.Audio
{

    [CreateAssetMenu(menuName = "Zone8/Enums/Audio/Track Definition")]
    public class ETrack : ScriptableObject
    {
        public AudioMixerGroup Track;
        [ReadOnly]
        public float MutedVolume;
    }
}
