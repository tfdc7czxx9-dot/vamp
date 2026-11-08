using System;
using System.IO;
using UnityEngine;
using Vamp.Core.Meta;
using Vamp.Platform;

namespace Vamp.Game
{
    /// <summary>Loads and atomically writes the save file; mirrors it to platform cloud storage.</summary>
    public sealed class SaveStore
    {
        private readonly string _path;
        private readonly IPlatformServices _platform;

        /// <summary>Creates a store at persistentDataPath/save.json.</summary>
        public SaveStore(IPlatformServices platform, string fileName = "save.json")
        {
            _platform = platform;
            _path = Path.Combine(Application.persistentDataPath, fileName);
        }

        /// <summary>Loads the local save, preferring the cloud copy if it has more progress. Never throws.</summary>
        public SaveData Load()
        {
            SaveData local = TryParse(ReadLocal());
            SaveData cloud = null;
            if (_platform != null && _platform.TryReadCloudSave(out var json)) cloud = TryParse(json);
            SaveData chosen = local;
            if (cloud != null && (local == null || Progress(cloud) > Progress(local))) chosen = cloud;
            chosen ??= new SaveData();
            MetaProgress.Migrate(chosen);
            return chosen;
        }

        private static long Progress(SaveData s) => s.totalRuns * 100000L + s.totalKills;

        private string ReadLocal()
        {
            try { return File.Exists(_path) ? File.ReadAllText(_path) : null; }
            catch (Exception e) { Debug.LogWarning("[Save] read failed: " + e.Message); return null; }
        }

        private static SaveData TryParse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try { return JsonUtility.FromJson<SaveData>(json); }
            catch (Exception e) { Debug.LogWarning("[Save] corrupt save ignored: " + e.Message); return null; }
        }

        /// <summary>Writes the save (temp file + replace) and pushes it to the cloud.</summary>
        public void Write(SaveData save)
        {
            string json = JsonUtility.ToJson(save, true);
            try
            {
                string tmp = _path + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(_path)) File.Replace(tmp, _path, null);
                else File.Move(tmp, _path);
            }
            catch (Exception e)
            {
                Debug.LogError("[Save] write failed: " + e.Message);
            }
            _platform?.WriteCloudSave(json);
        }
    }
}
