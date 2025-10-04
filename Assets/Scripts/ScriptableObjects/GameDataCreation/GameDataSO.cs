using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GameDataSO", menuName = "Game/GameData/New GameDataSO")]
public class GameDataSO : ScriptableObject
{
    public int gameCurrency = 0;

    public int limitOfUnlockableTurrets = 10;
    public  int limitOfSelectableTurrets = 5;
    public enum Class
    {
        Mechanic,
        Test
    }
    [SerializeField] private Class currentClass;

    [Header("Turrets")]
    public List<TurretBlueprint> allTurrets;          // All turrets in the game
    public List<TurretBlueprint> unlockedTurrets;     // Turrets player has unlocked
    public List<TurretBlueprint> selectedTurrets;     // Turrets currently selected

    // Checks if a turret is unlocked
    public bool IsUnlocked(TurretBlueprint turret) => unlockedTurrets.Contains(turret);

    public bool IsSelected (TurretBlueprint turret) => selectedTurrets.Contains(turret);
}
