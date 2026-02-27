using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using CatCatGo.Domain.ValueObjects;

namespace CatCatGo.Domain.Meta
{
    public class SaveData
    {
        public int Version;
        public long Timestamp;
        public Dictionary<string, object> PlayerData;
    }

    public class SaveManager
    {
        private const int SaveVersion = 1;
        private const string SaveFileName = "capybara_go_save.json";

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public Result Save(Dictionary<string, object> data)
        {
            var saveData = new SaveData
            {
                Version = SaveVersion,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                PlayerData = data,
            };

            try
            {
                string json = JsonConvert.SerializeObject(saveData);
                File.WriteAllText(SaveFilePath, json);
                return Result.Ok();
            }
            catch
            {
                return Result.Fail("Failed to save");
            }
        }

        public Result<SaveData> Load()
        {
            try
            {
                if (!File.Exists(SaveFilePath))
                    return Result.Fail<SaveData>("No save data found");

                string json = File.ReadAllText(SaveFilePath);
                var data = JsonConvert.DeserializeObject<SaveData>(json);

                if (data.Version != SaveVersion)
                    return Result.Fail<SaveData>($"Save version mismatch: expected {SaveVersion}, got {data.Version}");

                return Result.Ok(data);
            }
            catch
            {
                return Result.Fail<SaveData>("Failed to load save data");
            }
        }

        public Result DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                    File.Delete(SaveFilePath);
                return Result.Ok();
            }
            catch
            {
                return Result.Fail("Failed to delete save");
            }
        }

        public bool HasSave()
        {
            return File.Exists(SaveFilePath);
        }

        public long? GetLastSaveTime()
        {
            var result = Load();
            if (result.IsFail() || result.Data == null) return null;
            return result.Data.Timestamp;
        }
    }
}
