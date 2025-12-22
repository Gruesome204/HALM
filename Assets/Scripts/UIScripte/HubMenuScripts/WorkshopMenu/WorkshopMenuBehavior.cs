using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

public class WorkshopMenuBehavior : MonoBehaviour, IMenu
{
    private Label headline;
    private VisualElement availableTurrets;
    private VisualElement equippedTurrets;

    //UI Elements Turret Details
    private VisualElement detailsMainContainer;
    private VisualElement statsContainer;
    private Label informationTxt;
    private Button buySelectButton;
    private TurretBlueprint openTurretDetails;

    private Boolean turretUnlocked = false;
    private Boolean turretSelected = false;


    public VisualTreeAsset turretButtons;
    public VisualTreeAsset turretDetailsStats;

    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the SettingsMenu and adds it to openMenu List
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            Fill();
        }
        else
        {
            //Closes SettingsMenu and removes from openMenu List
            root.Q<VisualElement>("mainContainer").AddToClassList("turretChoiceMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            Clear();
        }
    }

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        headline = root.Q<Label>("headline");
        headline.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "headline"));

        availableTurrets = root.Q<VisualElement>("availableTurrets");
        equippedTurrets = root.Q<VisualElement>("equippedTurrets");

        detailsMainContainer = root.Q<VisualElement>("detailsMainContainer");
        //detailsMainContainer.AddToClassList("turretChoiceMenuSlideOut");

        statsContainer = root.Q<VisualElement>("statsContainer");

        informationTxt = root.Q<Label>("informationTxt");
        informationTxt.text = "";

        buySelectButton = root.Q<Button>("buySelectButton");
    }

    private void Fill()
    {
        foreach (var turret in GameManager.Instance.gameDataSO.allTurretBlueprints)
        {
            if (GameManager.Instance.gameDataSO.GetSelectedBlueprints().Contains<TurretBlueprint>(turret))
            {
                //Fill Space with all selected Turrets
                W_EquippedTurretsButtonBehavior availableTurret = new W_EquippedTurretsButtonBehavior(turretButtons, turret, this);
                equippedTurrets.Add(availableTurret.turretBorder);
            }
            else if (GameManager.Instance.gameDataSO.GetUnlockedBlueprints().Contains<TurretBlueprint>(turret))
            {
                //Fill Space with all unlocked Turrets
                W_AvailableTurretsButtonBejavior availableTurret = new W_AvailableTurretsButtonBejavior(turretButtons, turret, this);
                availableTurret.turretUnlocked = true;
                availableTurrets.Add(availableTurret.turretBorder);
            }
            else
            {
                //Fill Space with all locked turrets and mark them accordingly
                W_AvailableTurretsButtonBejavior availableTurret = new W_AvailableTurretsButtonBejavior(turretButtons, turret, this);
                availableTurrets.Add(availableTurret.turretBorder);
                availableTurret.cooldownCover.style.height = new Length(100, LengthUnit.Percent);
            }
        }
    }
    private void Clear()
    {
        availableTurrets.Clear();
        equippedTurrets.Clear();
        ClearTurretDetails();
    }

    public void FillTurretDetails(TurretBlueprint turretToBeDetailed,Boolean _turretUnlocked, Boolean _turretSelected)
    {
        ClearTurretDetails();
        openTurretDetails = turretToBeDetailed;
        turretUnlocked = _turretUnlocked;
        turretSelected = _turretSelected;

        buySelectButton.UnregisterCallback<ClickEvent>(TurretSelected);
        buySelectButton.UnregisterCallback<ClickEvent>(TurretUnlocked);
        buySelectButton.UnregisterCallback<ClickEvent>(TurretLocked);

        if (turretSelected)
        {
            //This Turrets is Selected and can only be removed from the List
            buySelectButton.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "selectedButton"));
            buySelectButton.RegisterCallback<ClickEvent>(TurretSelected);
        }
        else if (turretUnlocked)
        {
            //This Turret ist Unlocked, but not selected. If there is open space it can be added to the Selected Turrets
            buySelectButton.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "unlockedButton"));
            buySelectButton.RegisterCallback<ClickEvent>(TurretUnlocked);
        }
        else
        {
            //This Turret isn't unlocked. It has to be bought
            buySelectButton.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "lockedButton"));
            buySelectButton.RegisterCallback<ClickEvent>(TurretLocked);
        }

        TemplateContainer turretStats = turretDetailsStats.Instantiate();
        statsContainer.Add(turretStats);
        turretStats.style.flexGrow = 1;
        turretStats.Q<Label>("name").SetBinding("text", new LocalizedString($"TurretTranslation{openTurretDetails.turretName}", $"name"));

        var turretIcon = turretStats.Q<VisualElement>("icon");
        turretIcon.AddToClassList($"{openTurretDetails.turretName}Icon");

        FillCostValue(ref turretStats);
        FillDetailValue("fireRate", openTurretDetails.baseFireRate, ref turretStats);
        FillDetailValue("fireCountdown", openTurretDetails.BaseFireCountdown, ref turretStats);
        FillDetailValue("projectileSpeed", openTurretDetails.baseProjectileSpeed, ref turretStats);
        FillDetailValue("attackRange", openTurretDetails.baseAttackRange, ref turretStats);
        FillDetailValue("damage", openTurretDetails.baseAttackDamage, ref turretStats);
        FillDetailValue("knockbackStrength", openTurretDetails.baseKnockbackStrength, ref turretStats);
        FillDetailValue("knockbackDuration", openTurretDetails.baseKnockbackDuration, ref turretStats);

        detailsMainContainer.RemoveFromClassList("turretChoiceMenuSlideOut");
    }
    void FillDetailValue(string value, float turretValue, ref TemplateContainer container)
    {
        container.Q<Label>($"{value}Name").SetBinding("text", new LocalizedString("TurretTranslationCommon", $"{value}"));
        container.Q<Label>($"{value}").text = $"{turretValue}";
    }

    void FillCostValue(ref TemplateContainer container)
    {
        container.Q<Label>("costName").SetBinding("text", new LocalizedString("TurretTranslationCommon", $"cost"));
        StringTable table = LocalizationSettings.StringDatabase.GetTable("WorkshopMenuTranslationTable");

        string turretCost = new string("");
        foreach (var cost in openTurretDetails.buyCost)
        {
            var temporary = new string(table.GetEntry($"{cost.resourceType}").GetLocalizedString());

            turretCost = turretCost + $"<br>{temporary} {cost.amount}";
        }
        container.Q<Label>("cost").text = turretCost;
    }

    private void ClearTurretDetails()
    {
        detailsMainContainer.AddToClassList("turretChoiceMenuSlideOut");
        statsContainer.Clear();
        informationTxt.text = "";
    }
    void TurretSelected(ClickEvent evt)
    {
        GameManager.Instance.gameDataSO.DeselectBlueprint(openTurretDetails);
        Clear();
        Fill();
        FillTurretDetails(openTurretDetails, true, false);
    }
    void TurretUnlocked(ClickEvent evt)
    {
        if (GameManager.Instance.gameDataSO.SelectBlueprint(openTurretDetails))
        {
            Clear();
            Fill();
            FillTurretDetails(openTurretDetails, true, true);
        }
        else
        {
            informationTxt.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "cantSelectText"));
        }
    }
    void TurretLocked(ClickEvent evt)
    {
        if (TurretCanBeBought())
        {
            foreach (var cost in openTurretDetails.buyCost)
            {
                GameManager.Instance.gameDataSO.RemoveResource(cost.resourceType, cost.amount);
            }
            GameManager.Instance.gameDataSO.AddUnlockedBlueprint(openTurretDetails);
            Clear();
            Fill();
            FillTurretDetails(openTurretDetails, true, false);
        }
        else
        {
            informationTxt.SetBinding("text", new LocalizedString("WorkshopMenuTranslationTable", "cantBuyText"));
        }
    }

    private Boolean TurretCanBeBought()
    {
        var canBeBought = new Boolean();
        canBeBought = true;

        foreach (var cost in openTurretDetails.buyCost)
        {
            if (GameManager.Instance.gameDataSO.HasResource(cost.resourceType, cost.amount))
            {

            }
            else
            {
                canBeBought = false;
            }

        }
        return canBeBought;
    }
}
