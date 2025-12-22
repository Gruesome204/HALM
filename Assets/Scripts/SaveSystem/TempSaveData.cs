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
    public List<string> selectedBlueprintNames;
    public List<string> unlockedBuildMasterModifierNames;
    public List<string> selectedBuildMasterModifierNames;


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

        // Build Master Modifiers
        unlockedBuildMasterModifierNames = so.unlockedBuildMasterModifiers
            .Select(m => m.name)
            .ToList();

        selectedBuildMasterModifierNames = so.buildMasterModifiers
            .Select(m => m.name)
            .ToList();

        // Turrets
        unlockedBlueprintNames = so.unlockedBlueprints.Select(b => b.name).ToList();
        selectedBlueprintNames = so.selectedBlueprints.Select(b => b.name).ToList();
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
        unlockedBuildMasterModifierNames = new List<string>();
        selectedBuildMasterModifierNames = new List<string>();
        unlockedBlueprintNames = new List<string>();
        selectedBlueprintNames = new List<string>();
    }

}
