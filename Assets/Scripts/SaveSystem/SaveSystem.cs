using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string Folder = Path.Combine(Application.persistentDataPath, "Saves");
    private static readonly string PathMain = Path.Combine(Folder, "player.json");
    private static readonly string PathBackup = Path.Combine(Folder, "player_backup.json");

    // Expose main path for GameManager checks
    public static string MainSavePath => PathMain;

    // Set the current save version for your game
    public static readonly int CurrentSaveVersion = 1;

    public static TempSaveData Load()
    {
        // Ensure folder exists
        if (!Directory.Exists(Folder))
            Directory.CreateDirectory(Folder);

        // Try main save
        TempSaveData data = LoadFromFile(PathMain);
        if (IsValidSave(data))
            return data;

        // Try backup
        data = LoadFromFile(PathBackup);
        if (IsValidSave(data))
        {
            Debug.LogWarning("[SaveSystem] Loaded from backup.");
            return data;
        }

        // No valid save found → delete old files and return null
        Debug.LogWarning("[SaveSystem] No valid save found → creating new save on next Save() call.");
        DeleteSaveFiles();
        return null;
    }

    private static TempSaveData LoadFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
                return null;

            TempSaveData data = JsonUtility.FromJson<TempSaveData>(json);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load {path}: {e}");
            return null;
        }
    }

    private static bool IsValidSave(TempSaveData data)
    {
        if (data == null)
            return false;

        if (data.unlockedBlueprintNames == null || data.unlockedBlueprintNames.Count == 0)
            return false;

        // Version check
        if (data.saveVersion < CurrentSaveVersion)
            return false;

        return true;
    }

    public static void Save(TempSaveData data)
    {
        try
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            // Backup current save
            if (File.Exists(PathMain))
                File.Copy(PathMain, PathBackup, true);

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PathMain, json);
            Debug.Log($"[SaveSystem] Saved to {PathMain}");
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveSystem] Failed to save file: " + e);
        }
    }

    public static void DeleteSaveFiles()
    {
        try
        {
            if (File.Exists(PathMain))
                File.Delete(PathMain);

            if (File.Exists(PathBackup))
                File.Delete(PathBackup);

            Debug.Log("[SaveSystem] Save files deleted.");
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveSystem] Failed to delete save files: " + e);
        }
    }
}
