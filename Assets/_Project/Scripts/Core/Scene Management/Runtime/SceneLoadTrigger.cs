using Sirenix.OdinInspector;
using UnityEngine;
using Zone8.SOAP.ScriptableVariable;

namespace Zone8.SceneManagement
{
    public class SceneLoadTrigger : MonoBehaviour
    {
        [SerializeField] private ScriptableVariableRef<ESceneGroup> _sceneToLoad;
        [SerializeField] private bool _loadOnStart;

        private SceneManagementManager _sceneManager;

        private void Awake()
        {
            _sceneManager = FindAnyObjectByType<SceneManagementManager>();
        }

        private void Start()
        {
            if (_loadOnStart)
            {
                Load();
            }
        }

        [Button]
        public void Load()
        {
            if (!_sceneToLoad.IsNull) _sceneManager.Load(_sceneToLoad.Value);
        }
    }

}