using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int saveVersion = 1;

    public int gameCurrency;
    public int currentPlayerLevel;
    public GameDataSO.Class currentClass;

    public List<TurretType> unlockedTurrets;
    public List<TurretType> selectedTurrets;

    public GameData()
    {
        unlockedTurrets = new List<TurretType>();
        selectedTurrets = new List<TurretType>();
    }

    public GameData(GameDataSO so)
    {
        gameCurrency = so.gameCurrency;
        currentPlayerLevel = so.currentPlayerLevel;
        currentClass = so.currentClass;

        unlockedTurrets = new List<TurretType>(so.unlockedTurrets);
        selectedTurrets = new List<TurretType>(so.selectedTurrets);

        saveVersion = 1;
    }
}
