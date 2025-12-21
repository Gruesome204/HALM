using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Runtime Data that is only changed and is then saved into Data

[CreateAssetMenu(fileName = "New GameDataSO", menuName = "Game/GameData/New GameDataSO")]
public class GameDataSO : ScriptableObject
{
    public int saveVersion = 1;

    //All Settings Data
    public string localSelected;

    public float playTimeSeconds;

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
    [SerializeField]public List<TurretBlueprint> unlockedBlueprints;  // Blueprints player has unlocked
    [SerializeField] public List<TurretBlueprint> selectedBlueprints;

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

    //Remove a Ressource, but only if there is enough of it
    public Boolean TakeRessource(int ressourceNumber, int amount)
    {
        switch (ressourceNumber)
        {
            case 1:
                if (gameCurrency < amount)
                {
                    return false;
                }
                else
                {
                    gameCurrency = gameCurrency - amount;
                    return true;
                }
                break;
            case 2:
                if (woodResource < amount)
                {
                    return false;
                }
                else
                {
                    woodResource = woodResource - amount;
                    return true;
                }
                break;
            case 3:
                if (steinResource < amount)
                {
                    return false;
                }
                else
                {
                    steinResource = steinResource - amount;
                    return true;
                }
                break;
            case 4:
                if (metallResource < amount)
                {
                    return false;
                }
                else
                {
                    metallResource = metallResource - amount;
                    return true;
                }
                break;
            case 5:
                if (pulverResource < amount)
                {
                    return false;
                }
                else
                {
                    pulverResource = pulverResource - amount;
                    return true;
                }
                break;
        }
        return false;
    }

    //Add a Ressource
    public void GiveRessource(int ressourceNumber, int amount)
    {
        switch (ressourceNumber)
        {
            case 1:
                gameCurrency = gameCurrency + amount;
                break;
            case 2:
                woodResource = woodResource + amount;
                break;
            case 3:
                steinResource = steinResource + amount;
                break;
            case 4:
                metallResource = metallResource + amount;
                break;
            case 5:
                pulverResource = pulverResource + amount;
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
}
