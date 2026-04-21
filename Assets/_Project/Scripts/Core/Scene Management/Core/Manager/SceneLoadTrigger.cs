using Zone8.SOAP.ScriptableVariable;
using UnityEngine;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                Load();
            }
        }

        private void Start()
        {
            if (_loadOnStart)
            {
                Load();
            }
        }

        public void Load()
        {
            if (!_sceneToLoad.IsNull) _sceneManager.Load(_sceneToLoad.Value);
        }
    }

}