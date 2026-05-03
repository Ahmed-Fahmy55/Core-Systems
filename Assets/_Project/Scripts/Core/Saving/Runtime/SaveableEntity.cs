using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Zone8.Saving.Interfaces;

namespace Zone8.Saving.Runtime
{

    [ExecuteAlways]
    public class SaveableEntity : MonoBehaviour
    {
        [Tooltip("The unique ID is automatically generated in a scene file if " +
        "left empty. Do not set in a prefab unless you want all instances to " +
        "be linked.")]
        [SerializeField] string _id = "";

#if UNITY_EDITOR
        static Dictionary<string, SaveableEntity> s_globalLookup = new Dictionary<string, SaveableEntity>();
#endif

        public string GetID()
        {
            return _id;
        }

        public object CaptureState()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                state[saveable.GetType().ToString()] = saveable.CaptureState();
            }
            return state;
        }


        public void RestoreState(object state)
        {
            var stateDict = state as Dictionary<string, object>;

            if (stateDict == null) return;

            foreach (ISaveable saveable in GetComponents<ISaveable>())
            {
                string typeString = saveable.GetType().ToString();
                if (stateDict.ContainsKey(typeString))
                {
                    saveable.RestoreState(stateDict[typeString]);
                }
            }
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.IsPlaying(gameObject)) return;
            if (string.IsNullOrEmpty(gameObject.scene.path)) return;

            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty property = serializedObject.FindProperty("_id");

            if (string.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
            {
                property.stringValue = System.Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedProperties();
            }

            s_globalLookup[property.stringValue] = this;
        }

        private bool IsUnique(string candidate)
        {
            if (!s_globalLookup.ContainsKey(candidate)) return true;

            if (s_globalLookup[candidate] == this) return true;

            if (s_globalLookup[candidate] == null)
            {
                s_globalLookup.Remove(candidate);
                return true;
            }

            if (s_globalLookup[candidate].GetID() != candidate)
            {
                s_globalLookup.Remove(candidate);
                return true;
            }

            return false;
        }
#endif

    }
}