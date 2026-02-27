using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace CatCatGo.Infrastructure
{
    public static class JsonDataLoader
    {
        private const string BasePath = "_Project/Data/Json/";

        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = { new StringEnumConverter() }
        };

        public static T Load<T>(string fileName)
        {
            string raw = LoadRaw(fileName);
            if (raw == null) return default;
            return JsonConvert.DeserializeObject<T>(raw, Settings);
        }

        public static JObject LoadJObject(string fileName)
        {
            string raw = LoadRaw(fileName);
            if (raw == null) return null;
            return JObject.Parse(raw);
        }

        public static JArray LoadJArray(string fileName)
        {
            string raw = LoadRaw(fileName);
            if (raw == null) return null;
            return JArray.Parse(raw);
        }

        public static string LoadRaw(string fileName)
        {
            string path = BasePath + fileName.Replace(".json", "").Replace(".data", ".data");
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                Debug.LogError($"[JsonDataLoader] Failed to load: {path}");
                return null;
            }
            return textAsset.text;
        }
    }
}
