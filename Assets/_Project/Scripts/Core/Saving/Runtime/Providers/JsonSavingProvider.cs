using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zone8.Saving.Runtime.Providers
{
    public class JsonSavingProvider : SavingProviderBase
    {

        public override void Save(string saveFile, Dictionary<string, object> state)
        {
            string json = JsonConvert.SerializeObject(state, GetSettings());
            File.WriteAllText(GetPathFromSaveFile(saveFile), json);
        }

        public override Dictionary<string, object> Load(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path)) return new Dictionary<string, object>();

            string json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, GetSettings());
        }

        public override void Delete(string saveFile)
        {
            File.Delete(GetPathFromSaveFile(saveFile));
        }

        public override string GetPathFromSaveFile(string saveFile) =>
            Path.Combine(Application.persistentDataPath, saveFile + ".json");

        private JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto, // Preserves C# types like Dictionaries
                Formatting = Formatting.Indented        // Makes the file human-readable for debugging
            };
        }
    }
}