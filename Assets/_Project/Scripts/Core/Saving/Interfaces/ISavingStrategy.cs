using System.Collections.Generic;

namespace Zone8.Saving.Interfaces
{
    public interface ISavingStrategy
    {
        void Save(string saveFile, Dictionary<string, object> state);
        Dictionary<string, object> Load(string saveFile);
        void Delete(string saveFile);
        string GetPathFromSaveFile(string saveFile);
    }
}
