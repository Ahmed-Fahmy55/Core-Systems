using System.Collections.Generic;
using UnityEngine;

namespace Zone8.SceneManagement
{
    [CreateAssetMenu(menuName = "Zone8/Enums/SceneGroup")]
    public class ESceneGroup : ScriptableObject
    {
        public string DisplayName;
        public List<string> DependencyBundles;
    }
}

