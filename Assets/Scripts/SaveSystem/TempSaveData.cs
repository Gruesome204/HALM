using System.Collections.Generic;
using System.Linq;

[System.Serializable]

//File that is saved into a JSON
public class TempSaveData
{
    public int saveVersion = 1;

    //All Settings Data
    public string localSelected;
    public int musicTrack;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;

    // Player
    public int currentPlayerLevel;
    public GameDataSO.Class currentClass;

    // Resources
    public int gameCurrency;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;

    // --- Turrets ---
    public List<string> unlockedBlueprintNames; // store by unique name/ID
    public List<BuildMasterModifier> buildMasterModifiers;



    public TempSaveData(GameDataSO so)
    {
        saveVersion = 1;

        // Settings
        localSelected = so.localSelected;
        musicTrack = so.musicTrack;
        masterVolume = so.masterVolume;
        musicVolume = so.musicVolume;
        soundVolume = so.soundVolume;

        // Player
        currentPlayerLevel = so.currentPlayerLevel;
        currentClass = so.currentClass;

        // Resources
        gameCurrency = so.gameCurrency;
        woodResource = so.woodResource;
        steinResource = so.steinResource;
        metallResource = so.metallResource;
        pulverResource = so.pulverResource;

        // Turrets
        buildMasterModifiers = new List<BuildMasterModifier>(so.buildMasterModifiers);


        unlockedBlueprintNames = so.unlockedBlueprints.Select(b => b.name).ToList();
    }

    //Constructor
    public TempSaveData()
    {
        localSelected = "Default";
        musicTrack = 0;
        masterVolume = 1f;
        musicVolume = 1f;
        soundVolume = 1f;
        currentPlayerLevel = 1;
        currentClass = GameDataSO.Class.Mechanic;
        gameCurrency = 0;
        woodResource = 0;
        steinResource = 0;
        metallResource = 0;
        pulverResource = 0;
        buildMasterModifiers = new List<BuildMasterModifier>();
        unlockedBlueprintNames = new List<string>();
    }

}
