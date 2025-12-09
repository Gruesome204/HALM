using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

//File that is saved into a JSON
public class GameData
{
    public int saveVersion = 1;

    //All Settings Data
    public string localSelected;

    public int musicTrack;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;
    //Settings Data end

    public int currentPlayerLevel;
    public GameDataSO.Class currentClass;

    public int gameCurrency;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;

    public float additionalHealth;
    public float additionalMaxHealth;
    public float additionalRegen;
    public float additionalArmor;
    public float additionalMagicResistance;


    public List<TurretBlueprint> unlockedBlueprints;
    public List<TurretBlueprint> selectedBlueprints;
    public List<BuildMasterModifier> buildMasterModifiers;

    public GameData()
    {
        unlockedBlueprints = new List<TurretBlueprint>();
        selectedBlueprints = new List<TurretBlueprint>();
    }

    public GameData(GameDataSO so)
    {

        currentPlayerLevel = so.currentPlayerLevel;
        currentClass = so.currentClass;

        unlockedBlueprints = new List<TurretBlueprint>(so.GetUnlockedBlueprints());
        selectedBlueprints = new List<TurretBlueprint>(so.selectedBlueprints);

        saveVersion = 1;

        gameCurrency = so.gameCurrency;
        woodResource = so.woodResource;
        steinResource = so.steinResource;
        metallResource = so.metallResource;
        pulverResource = so.pulverResource;

        buildMasterModifiers = new List<BuildMasterModifier>();

    }
}
