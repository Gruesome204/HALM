using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class TU_ChoiceElementBehavior
{
    public VisualElement border;
    private Button button;

    private TurretUpgradeChoiceSO.UpgradeOption option;
    private TurretType turretType;
    private int turretLevel;

    public TU_ChoiceElementBehavior(VisualTreeAsset asset, TurretUpgradeChoiceSO.UpgradeOption _option, TurretType type, int level)
    {
        TemplateContainer choiceElement = asset.Instantiate();

        option = _option;
        turretType = type;
        turretLevel = level;

        border = choiceElement.Q<VisualElement>();
        button = choiceElement.Q<Button>();
        button.SetBinding("text", new LocalizedString("TurretUpgradeTranslationTable", "placeholder"));
        button.RegisterCallback<ClickEvent>(OnClicked);
    }

    void OnClicked(ClickEvent evt)
    {
        ApplyUpgrade(option);
        InGameMenuManager.Instance.ReturnToGame();
    }

    private void ApplyUpgrade(TurretUpgradeChoiceSO.UpgradeOption option)
    {
        if (TurretUpgradeChoiceManager.Instance == null)
        {
            Debug.LogError("TurretUpgradeChoiceManager instance is missing!");
            return;
        }
        if (TurretPlacementController.Instance == null)
        {
            Debug.LogError("TurretPlacementController instance is missing!");
            return;
        }


        // Use the passed-in turretType and level
        TurretUpgradeChoiceManager.Instance.ChooseUpgrade(turretType, turretLevel, option);
        Debug.Log($"Applied upgrade: {option.name} to {turretType} at level {turretLevel}");


        foreach (var turret in TurretPlacementController.Instance.GetActiveTurrets())
        {
            if (turret == null) continue;

            var levelBehaviour = turret.GetComponentInChildren<TurretLevelBehaviour>();
            if (levelBehaviour == null)
            {
                Debug.LogWarning($"Turret {turret.name} has no TurretLevelBehaviour component.");
                continue;
            }

            if (levelBehaviour.blueprint == null)
            {
                Debug.LogWarning($"Turret {turret.name} has no blueprint assigned in TurretLevelBehaviour.");
                continue;
            }

            if (levelBehaviour.blueprint.turretType == turretType)
            {
                levelBehaviour.ApplyUpgrades(turretLevel);
            }
        }
    }
}
