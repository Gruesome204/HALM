using System.IO;
using UnityEngine;
using System;

public static class SaveSystem
{
    private static readonly string Folder = Path.Combine(Application.persistentDataPath, "Saves");
    private static readonly string PathMain = Path.Combine(Folder, "player.json");
    private static readonly string PathBackup = Path.Combine(Folder, "player_backup.json");

    public static TempSaveData Load()
    {
        // Ensure folder exists
        if (!Directory.Exists(Folder))
            Directory.CreateDirectory(Folder);

        // Try main save
        TempSaveData data = LoadFromFile(PathMain);
        if (data != null)
            return data;

        // Try backup
        data = LoadFromFile(PathBackup);
        if (data != null)
        {
            Debug.LogWarning("[SaveSystem] Loaded from backup.");
            return data;
        }

        Debug.LogWarning("[SaveSystem] No valid save found → creating new TempSaveData.");
        return new TempSaveData();
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
