using System.Collections.Generic;

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

    //Game-Timer
    public float playTimeSeconds;

    // Player
    public int currentPlayerLevel;
    public GameDataSO.Class currentClass;

    // Resources
    public int gameCurrency;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;


    // --- Player upgrades ---
    public float additionalHealth;
    public float additionalMaxHealth;
    public float additionalRegen;
    public float additionalArmor;
    public float additionalMagicResistance;

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

        // Player upgrades
        additionalHealth = 0f;
        additionalMaxHealth = 0f;
        additionalRegen = 0f;
        additionalArmor = 0f;
        additionalMagicResistance = 0f;

        // Turrets
        unlockedBlueprintNames = new List<string>();
        foreach (var blueprint in so.GetUnlockedBlueprints())
        {
            if (blueprint != null)
                unlockedBlueprintNames.Add(blueprint.name);
        }

        buildMasterModifiers = new List<BuildMasterModifier>(so.buildMasterModifiers);
    }

    // Default constructor for new saves
    public TempSaveData()
    {
        unlockedBlueprintNames = new List<string>();
        buildMasterModifiers = new List<BuildMasterModifier>();
    }
}
