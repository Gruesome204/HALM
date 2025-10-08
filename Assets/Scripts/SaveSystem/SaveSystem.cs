using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/player.json";

    public static void Save(GameDataSO gameDataSO)
    {
        SaveGameData gameSaveData = new SaveGameData(gameDataSO );
        string json = JsonUtility.ToJson(gameSaveData, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved to " + path);
    }

    public static void Load(GameDataSO gameDataSO)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file found at " + path);
            return;
        }

        string json = File.ReadAllText(path);
        SaveGameData loadedData = JsonUtility.FromJson<SaveGameData>(json);
        loadedData.ApplyToSO(gameDataSO);
        Debug.Log("Loaded from " + path);
    }
}
