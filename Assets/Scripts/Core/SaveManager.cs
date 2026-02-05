using UnityEngine;
using System.IO;
using PrismPulse.Data;

namespace PrismPulse.Core
{
    public static class SaveManager
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "prism_save.json");
        public static void Save(GameSaveData data) { try { File.WriteAllText(SavePath, JsonUtility.ToJson(data)); } catch { } }
        public static GameSaveData Load() { if (!HasSaveFile()) return null; try { return JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SavePath)); } catch { return null; } }
        public static bool HasSaveFile() => File.Exists(SavePath);
        public static void DeleteSave() { if (HasSaveFile()) File.Delete(SavePath); }
    }
}