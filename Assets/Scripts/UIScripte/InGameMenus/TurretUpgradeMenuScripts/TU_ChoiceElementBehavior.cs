using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class TU_ChoiceElementBehavior
{
    public VisualElement border;
    private Button button;

    private TurretUpgradeChoiceSO choiceSO; 
    private TurretUpgradeChoiceSO.UpgradeOption option;
    private TurretType turretType;
    private int level;

    public TU_ChoiceElementBehavior(VisualTreeAsset asset, TurretType type,
        int level, TurretUpgradeChoiceSO choiceSO, TurretUpgradeChoiceSO.UpgradeOption _option)
    {
        TemplateContainer choiceElement = asset.Instantiate();

        this.turretType = type;
        this.level = level;
        this.choiceSO = choiceSO;
        this.option = _option;


        border = choiceElement.Q<VisualElement>("border");

        choiceElement.Q<Label>("name").SetBinding("text", new LocalizedString("", $"{option.name}"));

        choiceElement.Q<Label>("description").SetBinding("text", new LocalizedString("", $"{option.name}Description"));

        button = choiceElement.Q<Button>("turretUpgradeChoice_Btn");
        button.RegisterCallback<ClickEvent>(OnClicked);
    }

    void OnClicked(ClickEvent evt)
    {
        //Play a Click sound to give audio feedback to the Player
        InGameMenuManager.Instance.PlayClickSound("paper");

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

        TurretUpgradeChoiceManager.Instance.ChooseUpgrade(
        turretType,
        level,
        choiceSO,
        option
    );


        TurretLevelManager.Instance.ForceReapplyUpgrades(turretType);
    }
}
