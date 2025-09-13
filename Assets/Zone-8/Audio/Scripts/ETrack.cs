using Sirenix.OdinInspector;
using UnityEngine;

namespace Zone8.Audio
{

    [CreateAssetMenu(menuName = "Zone8/Enums/Audio/Track Definition")]
    public class ETrack : ScriptableObject
    {
        public string TrackName;
        public string Parameter;
        [ReadOnly]
        public float MutedVolume;
    }
}
