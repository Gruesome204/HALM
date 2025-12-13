using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameData/NewGameDataDefaults")]
public class GameDataDefaultsSO : ScriptableObject
{
    public string localSelected;
    public int musicTrack;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;

    public int currentPlayerLevel;
    public GameDataSO.Class currentClass;

    public int gameCurrency;
    public int woodResource;
    public int steinResource;
    public int metallResource;
    public int pulverResource;

    public List<TurretBlueprint> defaultUnlockedBlueprints;
    public List<BuildMasterModifier> defaultModifiers;
}
