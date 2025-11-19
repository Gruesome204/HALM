using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string path = Path.Combine(Application.persistentDataPath, "player.json");
    private static string backupPath = Path.Combine(Application.persistentDataPath, "player_backup.json");

    public static bool Save(PlayerData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);

            // Backup old save
            if (File.Exists(path))
                File.Copy(path, backupPath, true);

            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Save failed: " + e);
            return false;
        }
    }

    public static PlayerData Load()
    {
        try
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning("No save file – returning new PlayerData");
                return new PlayerData();
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Load failed, trying backup: " + e);

            // Try backup
            if (File.Exists(backupPath))
            {
                string json = File.ReadAllText(backupPath);
                return JsonUtility.FromJson<PlayerData>(json);
            }

            // No backup? Start new profile
            Debug.LogWarning("No backup found – returning new PlayerData");
            return new PlayerData();
        }
    }
}
