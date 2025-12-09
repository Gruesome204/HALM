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
    public List<TurretBlueprint> unlockedBlueprints;  // Blueprints player has unlocked
    public List<TurretBlueprint> selectedBlueprints;  // Blueprints currently selected

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

    // Check if blueprint is selected
    public bool IsSelected(TurretBlueprint blueprint)
    {
        return selectedBlueprints.Contains(blueprint);
    }

    // Get all unlocked blueprints
    public List<TurretBlueprint> GetUnlockedBlueprints()
    {
        return unlockedBlueprints.ToList();
    }

    // Optional: add blueprint to selection
    public bool TrySelectBlueprint(TurretBlueprint blueprint)
    {
        if (!IsUnlocked(blueprint)) return false; // Cannot select locked
        if (selectedBlueprints.Count >= limitOfSelectableTurrets) return false;

        selectedBlueprints.Add(blueprint);
        return true;
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
