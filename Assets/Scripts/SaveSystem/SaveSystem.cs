using System.IO;
using UnityEngine;
using System;

public static class SaveSystem
{
    private static readonly string Folder = Path.Combine(Application.persistentDataPath, "Saves");
    private static readonly string PathMain = System.IO.Path.Combine(Folder, "player.json");
    private static readonly string PathBackup = System.IO.Path.Combine(Folder, "player_backup.json");

    public static TempSaveData Load()
    {
        try
        {


            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            if (File.Exists(PathMain))
            {
                string json = File.ReadAllText(PathMain);
                TempSaveData data = JsonUtility.FromJson<TempSaveData>(json);

                if (data != null)
                    return data;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Main save corrupted: " + e);
        }

        // Try backup
        try
        {
            if (File.Exists(PathBackup))
            {
                string json = File.ReadAllText(PathBackup);
                TempSaveData data = JsonUtility.FromJson<TempSaveData>(json);

                if (data != null)
                {
                    Debug.LogWarning("Loaded from backup.");
                    return data;
                }
            }
        }
        catch { }

        Debug.LogWarning("No valid save found. Using new default GameData.");
        return new TempSaveData();


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
            Debug.LogError("Failed to delete save files: " + e);
        }
    }



    public static void Save(TempSaveData data)
    {
        try
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            string json = JsonUtility.ToJson(data, true);

            if (File.Exists(PathMain))
                File.Copy(PathMain, PathBackup, true); // backup before overwrite

            File.WriteAllText(PathMain, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save file: " + e);
        }
    }
}
