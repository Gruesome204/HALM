using System.Collections.Generic;
using UnityEngine;

public class BuildmasterModifyManager : MonoBehaviour
{
    public static BuildmasterModifyManager Instance { get; private set; }

    [SerializeField] GameDataSO gameDataSO;

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

    public List<BuildMasterModifier> availableBuildmasterModifiers = new List<BuildMasterModifier>();


    public void UseABuildmasterModifier(BuildMasterModifier modifier)
    {
        gameDataSO.buildMasterModifiers.Add(modifier);
        Debug.Log("Joy");
    }

    public List<BuildMasterModifier> GetBuildmasterModifiers() => availableBuildmasterModifiers;

}
