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
        // Use the passed-in turretType and level
        TurretUpgradeChoiceManager.Instance.ChooseUpgrade(turretType, turretLevel, option);

        Debug.Log($"Applied upgrade: {option.name} to {turretType} at level {turretLevel}");

        // Apply upgrades to all turrets of this type
        foreach (var turret in TurretPlacementController.Instance.GetActiveTurrets())
        {
            // Make sure turret is a GameObject
            if (turret == null) continue;
            // Access the actual TurretLevelBehaviour component directly
        
            if (turret != null && turret.GetComponentInChildren<TurretLevelBehaviour>().blueprint.turretType == turretType)
            {
                turret.GetComponentInChildren<TurretLevelBehaviour>().ApplyUpgrades(turretLevel);
            }       
        }
    }
}
