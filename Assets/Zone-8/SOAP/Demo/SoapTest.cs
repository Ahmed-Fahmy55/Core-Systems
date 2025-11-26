using Bltzo.SOAP.AssetVariable;
using UnityEngine;
using Zone8.SOAP.Events;
using Zone8.SOAP.RuntimeSet;
using Zone8.SOAP.ScriptableVariable;

public class SoapTest : MonoBehaviour, IGameEventListener<int>
{
    public GameEvent<int> IntEvent;
    public AssetVariableRef<Sprite> AssetVariableRef;

    public RuntimeSet<int> RuntimeSet;
    public ScriptableVariableRef<int> ScriptableVariableRef;

    public Sprite SpriteAsset;

    private async void Start()
    {
        RuntimeSet.Add(1);
        var handle = AssetVariableRef.LoadAssetAsync();
        await handle.Task;
        if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) return;

        SpriteAsset = handle.Result;
    }

    private void OnDestroy()
    {
        RuntimeSet?.Remove(1);
    }

    private void OnEnable()
    {
        IntEvent?.RegisterListener(this);
    }
    private void OnDisable()
    {
        IntEvent?.UnregisterListener(this);
    }

    public void OnEventRaised(int value)
    {
        Debug.Log(value);
    }
}
