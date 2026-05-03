using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Zone8.Saving.Interfaces;

namespace Zone8.Saving.Runtime.Providers
{
    public abstract class SavingProviderBase : MonoBehaviour, ISavingStrategy
    {
        [SerializeField] private string _defaultSavePath = "save";


        [Button]
        public void Save()
        {
            Dictionary<string, object> state = Load(_defaultSavePath);

            CaptureState(state);
            Save(_defaultSavePath, state);
        }

        [Button]
        public void Load()
        {
            RestoreState(Load(_defaultSavePath));
        }

        public abstract void Delete(string saveFile);

        public abstract Dictionary<string, object> Load(string saveFile);

        public abstract void Save(string saveFile, Dictionary<string, object> state);

        public abstract string GetPathFromSaveFile(string saveFile);


        private void CaptureState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsByType<SaveableEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                state[saveable.GetID()] = saveable.CaptureState();
            }
        }

        private void RestoreState(Dictionary<string, object> state)
        {
            foreach (SaveableEntity saveable in FindObjectsByType<SaveableEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                string id = saveable.GetID();
                if (state.ContainsKey(id))
                {
                    saveable.RestoreState(state[id]);
                }
            }
        }

    }
}
