using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Zone8.Saving.Runtime.Providers
{
    // Stores save data as UTF-8 JSON bytes. Replaces the deprecated BinaryFormatter
    // which is a known deserialization security vulnerability.
    public class BinariSavingProvider : SavingProviderBase
    {
        public override void Save(string saveFile, Dictionary<string, object> state)
        {
            string path = GetPathFromSaveFile(saveFile);
            string json = JsonConvert.SerializeObject(state, GetSettings());
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(json));
        }

        public override Dictionary<string, object> Load(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);
            if (!File.Exists(path)) return new Dictionary<string, object>();

            string json = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, GetSettings())
                   ?? new Dictionary<string, object>();
        }

        public override string GetPathFromSaveFile(string saveFile) =>
            Path.Combine(Application.persistentDataPath, saveFile + ".sav");

        public override void Delete(string saveFile) =>
            File.Delete(GetPathFromSaveFile(saveFile));

        private JsonSerializerSettings GetSettings() => new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None
        };
    }
}