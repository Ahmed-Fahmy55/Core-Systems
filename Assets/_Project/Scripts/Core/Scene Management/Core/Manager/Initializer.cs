using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class Initializer : MonoBehaviour
{
    [SerializeField] private SceneReference _sceneReference;
    [SerializeField] bool _isAdditive;
    [SerializeField] AssetReferenceT<GameObject> _persistant;

    private void Start()
    {
        if (_persistant != null)
        {
            var handlle = _persistant.InstantiateAsync();

            handlle.Completed += (op) =>
            {
                Load();
            };
        }
        else
        {
            Load();
        }
    }

    private void Load()
    {
        if (_sceneReference.State == SceneReferenceState.Regular)
        {
            var operation = SceneManager.LoadSceneAsync(_sceneReference.Path, _isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }
        else if (_sceneReference.State == SceneReferenceState.Addressable)
        {
            var sceneHandle = Addressables.LoadSceneAsync(_sceneReference.Address, _isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }
    }
}
