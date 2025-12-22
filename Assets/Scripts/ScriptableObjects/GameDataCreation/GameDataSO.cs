using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class UnlockableList<T>
{
    public List<T> all = new();
    public List<T> unlocked = new();
    public List<T> selected = new();
    public int selectionLimit = int.MaxValue;
}


//Runtime Data that is only changed and is then saved into Data

[CreateAssetMenu(fileName = "New GameDataSO", menuName = "Game/GameData/New GameDataSO")]
public class GameDataSO : ScriptableObject
{
    public int saveVersion = 1;

    //All Settings Data
    public string localSelected;

    public int musicTrack;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;
    //Settings Data end

    public int limitOfUnlockableTurrets = 10;
    public int limitOfSelectableTurrets = 5;
    public enum Class
    {
        Mechanic,
        Test
    }
    [SerializeField] public Class currentClass;
    public int currentPlayerLevel;

    [Header("Ressources")]
    public int gameCurrency = 0;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;

    [Header("Turrets")]
    public List<TurretBlueprint> allTurretBlueprints;
    [SerializeField] public List<TurretBlueprint> unlockedBlueprints;  // Blueprints player has unlocked
    [SerializeField] public List<TurretBlueprint> selectedBlueprints;

    [Header("PlayerStatsUpgrades")]
    public List<BuildMasterModifier> allBuildMasterModifiers;
    public List<BuildMasterModifier> buildMasterModifiers;// selected / active
    public List<BuildMasterModifier> unlockedBuildMasterModifiers;

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

    //Add a Blueprint to the unlocked List
    public void AddUnlockedBlueprint(TurretBlueprint blueprint)
    {
        unlockedBlueprints.Add(blueprint);
    }

    // Check if blueprint is selected
    public bool IsSelected(TurretBlueprint blueprint)
    {
        return selectedBlueprints.Contains(blueprint);
    }

    // Get all selected blueprints
    public List<TurretBlueprint> GetSelectedBlueprints()
    {
        return selectedBlueprints.ToList();
    }

    public void AddRemoveSelectedBlueprint(TurretBlueprint blueprint, Boolean add)
    {
        if (add)
        {
            selectedBlueprints.Add(blueprint);
        }
        else
        {
            selectedBlueprints.Remove(blueprint);
        }
    }

    public bool IsModifierSelected(BuildMasterModifier modifier)
    {
        return buildMasterModifiers.Contains(modifier);
    }

    public List<BuildMasterModifier> GetSelectedModifiers()
    {
        return buildMasterModifiers.ToList();
    }

    public bool IsModifierUnlocked(BuildMasterModifier modifier)
    {
        return unlockedBuildMasterModifiers.Contains(modifier);
    }

    public List<BuildMasterModifier> GetUnlockedModifiers()
    {
        return unlockedBuildMasterModifiers.ToList();
    }

    public void AddUnlockedModifier(BuildMasterModifier modifier)
    {
        if (!unlockedBuildMasterModifiers.Contains(modifier))
            unlockedBuildMasterModifiers.Add(modifier);
    }
    public bool DeselectModifier(BuildMasterModifier modifier)
    {
        return buildMasterModifiers.Remove(modifier);
    }

    public bool SelectModifier(BuildMasterModifier modifier)
    {
        if (!IsModifierUnlocked(modifier))
            return false;

        if (buildMasterModifiers.Contains(modifier))
            return false;

        buildMasterModifiers.Add(modifier);
        return true;
    }

    public bool HasResource(ResourceType type, int amount)
    {
        return GetResourceAmount(type) >= amount;
    }

    public void RemoveResource(ResourceType type, int amount)
    {
        SetResourceAmount(type, GetResourceAmount(type) - amount);
    }

    public void AddResource(ResourceType type, int amount)
    {
        SetResourceAmount(type, GetResourceAmount(type) + amount);
    }

    private int GetResourceAmount(ResourceType type)
    {
        return type switch
        {
            ResourceType.Currency => gameCurrency,
            ResourceType.Wood => woodResource,
            ResourceType.Stone => steinResource,
            ResourceType.Metal => metallResource,
            ResourceType.Pulver => pulverResource,
            _ => 0
        };
    }

    private void SetResourceAmount(ResourceType type, int value)
    {
        value = Mathf.Max(0, value); // clamp to prevent negatives

        switch (type)
        {
            case ResourceType.Currency:
                gameCurrency = value;
                break;
            case ResourceType.Wood:
                woodResource = value;
                break;
            case ResourceType.Stone:
                steinResource = value;
                break;
            case ResourceType.Metal:
                metallResource = value;
                break;
            case ResourceType.Pulver:
                pulverResource = value;
                break;
        }
    }

    // Add a blueprint to selected list
    public bool SelectBlueprint(TurretBlueprint blueprint)
    {
        if (selectedBlueprints.Count >= limitOfSelectableTurrets)
            return false; // Can't select more than allowed

        if (!IsUnlocked(blueprint))
            return false; // Can only select unlocked turrets

        if (!selectedBlueprints.Contains(blueprint))
        {
            selectedBlueprints.Add(blueprint);
            return true;
        }

        return false; // Already selected
    }

    // Remove a blueprint from selected list
    public bool DeselectBlueprint(TurretBlueprint blueprint)
    {
        return selectedBlueprints.Remove(blueprint);
    }

    // Set the selected blueprints all at once (e.g., from save)
    public void SetSelectedBlueprints(List<TurretBlueprint> blueprints)
    {
        // Only keep unlocked blueprints and respect selection limit
        selectedBlueprints = blueprints
            .Where(b => IsUnlocked(b))
            .Take(limitOfSelectableTurrets)
            .ToList();
    }


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

    public GameDataSO ApplySave(TempSaveData save)
    {
        // Settings
        localSelected = save.localSelected;
        musicTrack = save.musicTrack;
        masterVolume = save.masterVolume;
        musicVolume = save.musicVolume;
        soundVolume = save.soundVolume;

        // Player
        currentPlayerLevel = save.currentPlayerLevel;
        currentClass = save.currentClass;

        // Resources
        gameCurrency = save.gameCurrency;
        woodResource = save.woodResource;
        steinResource = save.steinResource;
        metallResource = save.metallResource;
        pulverResource = save.pulverResource;

        // Player upgrades
        buildMasterModifiers = new List<BuildMasterModifier>(save.buildMasterModifiers);

        // Turrets
        unlockedBlueprints = allTurretBlueprints
            .Where(b => save.unlockedBlueprintNames.Contains(b.name))
            .ToList();

        return this;
    }

    public TempSaveData ToSaveData()
    {
        return new TempSaveData(this);
    }
}
