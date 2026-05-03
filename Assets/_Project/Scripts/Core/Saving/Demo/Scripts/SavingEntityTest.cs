using System.Collections.Generic;
using UnityEngine;
using Zone8.Saving;
using Zone8.Saving.Interfaces;

public class SavingEntityTest : MonoBehaviour, ISaveable
{
    [SerializeField] float health = 100f;

    public object CaptureState()
    {
        var state = new Dictionary<string, object>();
        state["health"] = health;
        state["position"] = new SerializableVector3(transform.position);
        return state;
    }

    public void RestoreState(object state)
    {
        var stateDict = (Dictionary<string, object>)state;
        health = System.Convert.ToSingle(stateDict["health"]);
        SerializableVector3 pos = (SerializableVector3)stateDict["position"];
        transform.position = pos.ToVector();
    }

    [Sirenix.OdinInspector.Button]
    public void MoveAndDamage()
    {
        transform.position = Random.insideUnitSphere * 5f;
        health = Random.Range(0, 100);
    }
}