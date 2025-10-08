using System.Collections.Generic;
using static GameDataSO;

[System.Serializable]
public class SaveGameData
{
    public int playerLevel;
    public Class playerClass;
    public int gameCurrency;
    public List<TurretBlueprint> unlockedTurrets;
    public List<TurretBlueprint> selectedTurrets;

    public SaveGameData(GameDataSO gameDataSO)
    {
        playerLevel = gameDataSO.currentPlayerLevel;
        playerClass = gameDataSO.currentClass;
        gameCurrency = gameDataSO.gameCurrency;
        unlockedTurrets = gameDataSO.unlockedTurrets;
        selectedTurrets = gameDataSO.selectedTurrets;


    }

    public void ApplyToSO(GameDataSO gameDataSO)
    {
        gameDataSO.currentPlayerLevel = playerLevel;    
        gameDataSO.currentClass = playerClass;
        gameDataSO.gameCurrency = gameCurrency;
        gameDataSO.unlockedTurrets = unlockedTurrets;
        gameDataSO.selectedTurrets = selectedTurrets;
    }
}