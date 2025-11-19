using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public int gameCurrency;
    public int currentPlayerLevel;
    public GameDataSO.Class currentClass;

    public List<TurretType> unlockedTurrets;
    public List<TurretType> selectedTurrets;

    public PlayerData()
    {
        unlockedTurrets = new List<TurretType>();
        selectedTurrets = new List<TurretType>();
    }
}
