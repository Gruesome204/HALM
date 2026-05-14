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


    private void OnDestroy()
    {
    }

    public void SyncWithCurrentLevel()
    {
        if (turretBehaviour?.turretBlueprint == null || turretStats == null)
            return;

        int level = TurretLevelManager.Instance.GetLevel(
            turretBehaviour.turretBlueprint.turretType
        );

        TurretModifier upgrade =
            TurretUpgradeChoiceManager.Instance != null
                ? TurretUpgradeChoiceManager.Instance.GetCombinedModifier(
                    turretBehaviour.turretBlueprint.turretType)
                : null;

        TurretGlobalModifierManager global =
            TurretGlobalModifierManager.Instance;

        turretStats.RecalculateStats(
            turretBehaviour,
            turretBehaviour.turretBlueprint,
            level,
            upgrade,
            global
        );
    }

    public void ApplyUpgrades(int level)
    {
        if (turretBehaviour?.turretBlueprint == null || turretStats == null)
            return;

        TurretModifier upgrade =
            TurretUpgradeChoiceManager.Instance != null
                ? TurretUpgradeChoiceManager.Instance.GetCombinedModifier(
                    turretBehaviour.turretBlueprint.turretType)
                : null;

        TurretGlobalModifierManager global =
            TurretGlobalModifierManager.Instance;

        turretStats.RecalculateStats(
            turretBehaviour,
            turretBehaviour.turretBlueprint,
            level,
            upgrade,
            global
        );
    }
}
