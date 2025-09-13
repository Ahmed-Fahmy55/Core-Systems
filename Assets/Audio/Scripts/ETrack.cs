using Sirenix.OdinInspector;
using UnityEngine;

namespace Bltzo.Audio
{

    [CreateAssetMenu(menuName = "Bltzo/Enums/Audio/Track Definition")]
    public class ETrack : ScriptableObject
    {
        public string TrackName;
        public string Parameter;
        [ReadOnly]
        public float MutedVolume;
    }
}
