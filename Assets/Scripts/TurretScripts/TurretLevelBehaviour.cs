using UnityEngine;

public class TurretLevelBehaviour : MonoBehaviour
{
    [Header("References")]
    public TurretBehaviour turretBehaviour;
    private void OnEnable()
    {
        SyncWithCurrentLevel();
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
    }


    private void OnDestroy()
    {
    }

    public void SyncWithCurrentLevel()
    {
        if (turretBehaviour?.turretBlueprint == null) return;

        int level = TurretLevelManager.Instance.GetLevel(
            turretBehaviour.turretBlueprint.turretType
        );

        turretBehaviour.RecalculateStats(level);
    }


    public void ApplyUpgrades(int level)
    {
        turretBehaviour.RecalculateStats(level);
    }
}
