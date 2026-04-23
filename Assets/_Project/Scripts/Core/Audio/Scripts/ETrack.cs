using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace Zone8.Audio
{

    [CreateAssetMenu(menuName = "Enums/Audio/Track Definition")]
    public class ETrack : ScriptableObject
    {
        public AudioMixerGroup Track;

        [Title("Remote Control")]
        [Tooltip("The exact name of the Exposed Parameter in the Audio Mixer.")]
        public string ExposedParameterName;

        [ReadOnly]
        public float MutedVolume;
    }
}
