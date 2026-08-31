using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBehaviour turretBehaviour;
    public TurretStats turretStats;

    private void OnEnable()
    {
        if (TurretGlobalModifierManager.Instance != null)
            TurretGlobalModifierManager.Instance.OnModifiersChanged += HandleGlobalModifiersChanged;
    }

    private void OnDisable()
    {
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

        SyncWithCurrentLevel();
    }

    public void SyncWithCurrentLevel()
    {
        // Use the public property instead of direct field access
        if (turretBehaviour?.TurretBlueprint == null || turretStats == null)
            return;

        int level = TurretLevelManager.Instance.GetLevel(
            turretBehaviour.TurretBlueprint.turretType
        );

        TurretModifier upgrade =
            TurretUpgradeChoiceManager.Instance != null
                ? TurretUpgradeChoiceManager.Instance.GetCombinedModifier(
                    turretBehaviour.TurretBlueprint.turretType)
                : null;

        TurretGlobalModifierManager global =
            TurretGlobalModifierManager.Instance;

        turretStats.RecalculateStats(
            turretBehaviour,
            turretBehaviour.TurretBlueprint,
            level,
            upgrade,
            global
        );
    }

    public void ApplyUpgrades(int level)
    {
        if (turretBehaviour?.TurretBlueprint == null || turretStats == null)
            return;

        TurretModifier upgrade =
            TurretUpgradeChoiceManager.Instance != null
                ? TurretUpgradeChoiceManager.Instance.GetCombinedModifier(
                    turretBehaviour.TurretBlueprint.turretType)
                : null;

        TurretGlobalModifierManager global =
            TurretGlobalModifierManager.Instance;

        turretStats.RecalculateStats(
            turretBehaviour,
            turretBehaviour.TurretBlueprint,
            level,
            upgrade,
            global
        );
    }
}