using Zone8.SOAP.AssetVariable;
using Zone8.SOAP.Events;
using UnityEngine;

public class SoapTest : MonoBehaviour, IGameEventListener<int>
{
    public GameEvent<int> IntEvent;
    public SoAssetRef assetRef;

    /*    public RuntimeSet<int> RuntimeSet;
        public ScriptableVariableRef<int> ScriptableVariableRef;



        private void Start()
        {
            RuntimeSet.Add(1);
        }
        private void OnDestroy()
        {
            RuntimeSet.Remove(1);
        }*/

    private void OnEnable()
    {
        IntEvent.RegisterListener(this);
    }
    private void OnDisable()
    {
        IntEvent.UnregisterListener(this);
    }
    private void Start()
    {
        IntEvent.Raise(1);
    }
    public void OnEventRaised(int value)
    {
        Debug.Log(value);
    }
}
