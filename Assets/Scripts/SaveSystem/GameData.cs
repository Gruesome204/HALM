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
    public int schieﬂpulverResource;

    public float additionalHealth;
    public float additionalMaxHealth;
    public float additionalRegen;
    public float additionalArmor;
    public float additionalMagicResistance;


    public List<TurretType> unlockedTurrets;
    public List<TurretType> selectedTurrets;

    public GameData()
    {
        unlockedTurrets = new List<TurretType>();
        selectedTurrets = new List<TurretType>();
    }

    public GameData(GameDataSO so)
    {

        currentPlayerLevel = so.currentPlayerLevel;
        currentClass = so.currentClass;

        unlockedTurrets = new List<TurretType>(so.unlockedTurrets);
        selectedTurrets = new List<TurretType>(so.selectedTurrets);

        saveVersion = 1;

        gameCurrency = so.gameCurrency;
        woodResource = so.woodResource;
        steinResource = so.steinResource;
        metallResource = so.metallResource;
        schieﬂpulverResource = so.schieﬂpulverResource;

        additionalHealth = so.additionalHealth;
        additionalMaxHealth = so.additionalMaxHealth;
        additionalRegen = so.additionalRegen;
        additionalArmor = so.additionalArmor;
        additionalMagicResistance = so.additionalMagicResistance;


    }
}
