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
    public List<TurretType> allTurrets;          // All turrets in the game
    public List<TurretType> unlockedTurrets;     // Turrets player has unlocked
    public List<TurretType> selectedTurrets;     // Turrets currently selected

    public TurretBlueprint GetBlueprintByType(TurretType type)
    {
        return allTurretBlueprints.FirstOrDefault(t => t.turretType == type);
    }
    public TurretBlueprint GetBlueprint(TurretType type)
    {
        return allTurretBlueprints.Find(t => t.turretType == type);
    }
    public bool IsUnlocked(TurretType type)
    {
        return unlockedTurrets.Contains(type);
    }

    public bool IsSelected(TurretType type)
    {
        return selectedTurrets.Contains(type);
    }

    public List<TurretBlueprint> GetUnlockedBlueprints()
    {
        return allTurretBlueprints
            .Where(t => unlockedTurrets.Contains(t.turretType))
            .ToList();
    }

    [Header("Ressources")]
    public int gameCurrency = 0;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;

    [Header("PlayerStatsUpgrades")]
    public List<BuildMasterModifier> buildMasterModifiers;

}
