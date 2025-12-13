using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Runtime Data that is only changed and is then saved into Data

[CreateAssetMenu(fileName = "New GameDataSO", menuName = "Game/GameData/New GameDataSO")]
public class GameDataSO : ScriptableObject
{
    //All Settings Data
    public string localSelected;

    public int musicTrack;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;
    //Settings Data end


    public int limitOfUnlockableTurrets = 10;
    public  int limitOfSelectableTurrets = 5;
    public enum Class
    {
        Mechanic,
        Test
    }
    [SerializeField] public Class currentClass;
    public int currentPlayerLevel;

    [Header("Turrets")]
    public List<TurretBlueprint> allTurretBlueprints;
    [SerializeField]private List<TurretBlueprint> unlockedBlueprints;  // Blueprints player has unlocked

    public TurretBlueprint GetBlueprintByType(TurretType type)
    {
        return allTurretBlueprints.FirstOrDefault(t => t.turretType == type);
    }
    public TurretBlueprint GetBlueprint(TurretType type)
    {
        return allTurretBlueprints.Find(t => t.turretType == type);
    }
    // Check if blueprint is unlocked
    public bool IsUnlocked(TurretBlueprint blueprint)
    {
        return unlockedBlueprints.Contains(blueprint);
    }


    // Get all unlocked blueprints
    public List<TurretBlueprint> GetUnlockedBlueprints()
    {
        return unlockedBlueprints.ToList();
    }

    [Header("Ressources")]
    public int gameCurrency = 0;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;

    [Header("PlayerStatsUpgrades")]
    public List<BuildMasterModifier> buildMasterModifiers;


    public void ResetToDefaults(GameDataDefaultsSO defaults)
    {
        localSelected = defaults.localSelected;
        musicTrack = defaults.musicTrack;
        masterVolume = defaults.masterVolume;
        musicVolume = defaults.musicVolume;
        soundVolume = defaults.soundVolume;

        currentPlayerLevel = defaults.currentPlayerLevel;
        currentClass = defaults.currentClass;

        gameCurrency = defaults.gameCurrency;
        woodResource = defaults.woodResource;
        steinResource = defaults.steinResource;
        metallResource = defaults.metallResource;
        pulverResource = defaults.pulverResource;

        unlockedBlueprints = new List<TurretBlueprint>(defaults.defaultUnlockedBlueprints);
        buildMasterModifiers = new List<BuildMasterModifier>(defaults.defaultModifiers);
    }


}
