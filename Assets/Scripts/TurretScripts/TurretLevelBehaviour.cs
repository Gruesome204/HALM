using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBlueprint blueprint;
    private TurretBehaviour turretBehaviour;
    private void OnEnable()
    {
        if (blueprint != null && TurretLevelManager.Instance != null)
            SyncWithCurrentLevel();


        if (TurretGlobalModifierManager.Instance != null)
            TurretGlobalModifierManager.Instance.OnModifiersChanged += HandleGlobalModifiersChanged;
    }

    private void OnDisable()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnLevelUp -= HandleLevelUp;

        if (TurretGlobalModifierManager.Instance != null)
            TurretGlobalModifierManager.Instance.OnModifiersChanged -= HandleGlobalModifiersChanged;
    }

    private void HandleGlobalModifiersChanged()
    {
        SyncWithCurrentLevel();
    }

    private void Awake()
    {
        turretBehaviour = GetComponent<TurretBehaviour>();
        if (turretBehaviour == null)
        {
            Debug.LogError($"{name} has no TurretBehaviour attached!");
            return;
        }
    }

    private void Start()
    {

        // Sync upgrades with current level
        SyncWithCurrentLevel();

    }
        
    private void OnDestroy()
    {
        if (TurretLevelManager.Instance != null)
            TurretLevelManager.Instance.OnLevelUp -= HandleLevelUp;
    }

    public void SyncWithCurrentLevel()
    {
        int currentLevel = TurretLevelManager.Instance.GetLevel(blueprint.turretType);
        ApplyUpgrades(currentLevel);
    }

    private void HandleLevelUp(TurretType type, int newLevel)
    {
        if (type == blueprint.turretType)
            ApplyUpgrades(newLevel);
    }

    public void ApplyUpgrades(int level)
    {
        turretBehaviour.RecalculateStats();
    }


#if UNITY_EDITOR
    [ContextMenu("Give XP Test")]
    private void DebugGiveXP()
    {
        TurretLevelManager.Instance.AddXP(blueprint.turretType, 100f);
    }
#endif
}
