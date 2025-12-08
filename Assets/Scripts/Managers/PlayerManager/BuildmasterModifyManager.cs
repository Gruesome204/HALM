using System.Collections.Generic;
using UnityEngine;

public class BuildmasterModifyManager : MonoBehaviour
{
    public static BuildmasterModifyManager Instance { get; private set; }

    [SerializeField] GameDataSO gameDataSO;

    public List<BuildMasterModifier> availableBuildmasterModifiers = new List<BuildMasterModifier>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[BuildmasterModifyManager] Duplicate instance detected. Destroying this object.");
            Destroy(gameObject);
        }
    }



    public void UseABuildmasterModifier(BuildMasterModifier modifier)
    {
        gameDataSO.buildMasterModifiers.Add(modifier);
    }

    public void RemoveABuildmasterModifier(BuildMasterModifier modifier)
    {
        gameDataSO.buildMasterModifiers.Remove(modifier);
    }

    public List<BuildMasterModifier> GetBuildmasterModifiers() => availableBuildmasterModifiers;
    public List<BuildMasterModifier> GetAppliedBuildmasterModifiers() => gameDataSO.buildMasterModifiers;


}
