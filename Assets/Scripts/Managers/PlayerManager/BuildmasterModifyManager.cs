using System.Collections.Generic;
using UnityEngine;

public class BuildmasterModifyManager : MonoBehaviour
{
    public static BuildmasterModifyManager Instance { get; private set; }

    [SerializeField] GameDataSO gameDataSO;

    public List<BuildMasterModifier> availableBuildmasterModifiers = new List<BuildMasterModifier>();


    public void UseABuildmasterModifier(BuildMasterModifier.Modifier modifier)
    {
        //Retracts Gold cost
        gameDataSO.gameCurrency = gameDataSO.gameCurrency + ((int)modifier.goldCost);

        //Add gained stats
        gameDataSO.additionalHealth = gameDataSO.additionalHealth + modifier.additionalHealth;
        gameDataSO.additionalMaxHealth = gameDataSO.additionalMaxHealth + modifier.additionalMaxHealth;
        gameDataSO.additionalRegen = gameDataSO.additionalRegen + modifier.additionalRegen;
        gameDataSO.additionalArmor = gameDataSO.additionalArmor + modifier.additionalArmor;
        gameDataSO.additionalMagicResistance = gameDataSO.additionalMagicResistance + modifier.additionalMagicResistance;
    }
}
