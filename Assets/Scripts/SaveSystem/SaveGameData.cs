using System.Collections.Generic;
using System.Linq;
using static GameDataSO;

[System.Serializable]
public class SaveGameData
{
    public int playerLevel;
    public Class playerClass;
    public int gameCurrency;
    public List<TurretType> unlockedTurretTypes;
    public List<TurretType> selectedTurretTypes;

    public SaveGameData(GameDataSO gameDataSO)
    {
        playerLevel = gameDataSO.currentPlayerLevel;
        playerClass = gameDataSO.currentClass;
        gameCurrency = gameDataSO.gameCurrency;
        unlockedTurretTypes = gameDataSO.unlockedTurrets.ToList();
        selectedTurretTypes = gameDataSO.selectedTurrets.ToList();


    }

    public void ApplyToSO(GameDataSO gameDataSO)
    {
        gameDataSO.currentPlayerLevel = playerLevel;    
        gameDataSO.currentClass = playerClass;
        gameDataSO.gameCurrency = gameCurrency;
        gameDataSO.unlockedTurrets = unlockedTurretTypes.ToList();
        gameDataSO.selectedTurrets = selectedTurretTypes.ToList();
    }
}