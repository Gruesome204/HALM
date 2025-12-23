using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameData/NewGameDataDefaults")]
public class GameDataDefaultsSO : ScriptableObject
{
    [Header("Settings")]
    public string localSelected;
    public int musicTrack;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;

    [Header("Player")]
    public int currentPlayerLevel;
    public GameDataSO.Class currentClass;

    [Header("Resources")]
    public int gameCurrency;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;

    [Header("Turrets")]
    public List<TurretBlueprint> allTurretBlueprints;            // All turrets in the game
    public List<TurretBlueprint> defaultUnlockedBlueprints;      // Which turrets are unlocked by default
    public List<TurretBlueprint> defaultSelectedBlueprints;      // Optional: which are selected by default

    [Header("BuildMasterModifiers")]
    public List<BuildMasterModifier> allBuildMasterModifiers;    // All modifiers in the game
    public List<BuildMasterModifier> defaultModifiers;           // Which modifiers are unlocked by default
    public List<BuildMasterModifier> defaultSelectedModifiers;   // Optional: which are selected by default
}
