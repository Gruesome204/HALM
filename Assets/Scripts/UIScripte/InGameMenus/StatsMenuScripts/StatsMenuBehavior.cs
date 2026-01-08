using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UIElements;

public class StatsMenuBehavior : MonoBehaviour, IMenu
{
    private UIDocument uIDocument;

    private Label statsMenuHeadline;

    private Button resumeButton;
    private Button pauseMenuButton;
    private Button charackterButton;
    private Button modifierButton;
    private Button towerButton;

    private VisualElement statsContainer; 

    public VisualTreeAsset turretListElementAsset;
    public VisualTreeAsset modifierListElementAsset;
    public VisualTreeAsset turretDetailsAsset;
    public VisualTreeAsset charackterDetailsAsset;


    public void OpenOrClose(Boolean open)
    {
        //Gets the UiDocument and checks weather the Menu should opened or closed
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (open)
        {
            //Open the StatsMenu, adds to openMenu List and sets Game to paused
            root.Q<VisualElement>("mainContainer").RemoveFromClassList("pauseMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Add(this.gameObject);
            GameManager.Instance.ChangeState(GameManager.GameState.Paused);
            ClearList();

            //This ensures that the picture in the back can grow as large as possible while keeping it's correct scale :D
            //It can only be run when the Menu is actually on screen, otherwise the numbers go hairwire O.o
            // I wanted to run this once in the Enable Method, but due to the above that didn't work :C
            //I'm leaving it like this for now, it works fine so just close your eyes and look elswhere Lukas XD
            var container = root.Q<VisualElement>("sideContainer");
            container.style.width = (int)((int)container.resolvedStyle.height / (float)1.31);
        }
        else
        {
            //Closes StatsMenu, removes from openMenu List, and clears all lists
            root.Q<VisualElement>("mainContainer").AddToClassList("pauseMenuSlideOut");
            InGameMenuManager.Instance.openMenus.Remove(this.gameObject);
            ClearList();
        }
    }
    void OnEnable()
    {
        uIDocument = GetComponent<UIDocument>();

        //Connect and set Stats Menu Headline
        statsMenuHeadline = uIDocument.rootVisualElement.Q<Label>("statsMenuHeadline");
        statsMenuHeadline.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "statsMenuHeadline"));

        //Connecting to the Stats Container as it will be used often
        statsContainer = uIDocument.rootVisualElement.Q<VisualElement>("statsContainer");

        //Connecting all Buttons and settig their Texts and Functions
        resumeButton = uIDocument.rootVisualElement.Q<Button>("resumeButton");
        resumeButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "resumeButton"));
        resumeButton.RegisterCallback<ClickEvent>(OnResumeBtnClicked);

        pauseMenuButton = uIDocument.rootVisualElement.Q<Button>("pauseMenuButton");
        pauseMenuButton.SetBinding("text", new LocalizedString("PauseMenuTranslationTable", "pauseMenuHeadline"));
        pauseMenuButton.RegisterCallback<ClickEvent>(OnPauseBtnClicked);

        charackterButton = uIDocument.rootVisualElement.Q<Button>("charackterButton");
        charackterButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "charackterButton"));
        charackterButton.RegisterCallback<ClickEvent>(OnCharackterBtnClicked);

        modifierButton = uIDocument.rootVisualElement.Q<Button>("relicsButton");
        modifierButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "modifierButton"));
        modifierButton.RegisterCallback<ClickEvent>(OnRelicsBtnClicked);

        towerButton = uIDocument.rootVisualElement.Q<Button>("towerButton");
        towerButton.SetBinding("text", new LocalizedString("StatsMenuTranslationTable", "towerButton"));
        towerButton.RegisterCallback<ClickEvent>(OnTowerBtnClicked);
    }


    void OnResumeBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.ReturnToGame();
    }
    void OnPauseBtnClicked(ClickEvent evt)
    {
        InGameMenuManager.Instance.CloseAllMenus();
        InGameMenuManager.Instance.OpenOrCloseOneMenu("PauseMenuDoc", true);
    }
    void OnCharackterBtnClicked(ClickEvent evt)
    {
        ClearList();

        statsContainer.RemoveFromClassList("turretChoiceMenuSlideOut");

        TemplateContainer charackterDetails = turretDetailsAsset.Instantiate();
        charackterDetails.style.height = new Length(100, LengthUnit.Percent);
        statsContainer.Add(charackterDetails);

        charackterDetails.Q<Label>($"name").SetBinding("text", new LocalizedString("ClassTranslationCommon", $"Mechanic"));


        var turretIcon = charackterDetails.Q<VisualElement>("icon");
        turretIcon.AddToClassList($"MechanicIcon");

        charackterDetails.Q<Label>($"costName").text = "";
        charackterDetails.Q<Label>($"cost").text = $"";

        var player = FindFirstObjectByType<PlayerStats>();

        FillCharacterDetailValue("playerHitpoints", player.currentHealth, ref charackterDetails);
        FillCharacterDetailValue("playerArmor", player.currentArmor, ref charackterDetails);
        FillCharacterDetailValue("playerMoveSpeed", player.currentMoveSpeed, ref charackterDetails);
        FillCharacterDetailValue("playerBuildRadius", TurretPlacementController.Instance.placementRadius, ref charackterDetails);

    }
    void OnRelicsBtnClicked(ClickEvent evt)
    {
        ClearList();
        var scroll = new ScrollView();
        statsContainer.Add(scroll);

        foreach (var modifier in GameManager.Instance.gameDataSO.GetSelectedModifiers())
        {
            TemplateContainer modifierElement = modifierListElementAsset.Instantiate();

            modifierElement.Q<Label>("name").SetBinding("text", new LocalizedString($"BMTranslations{modifier.options.name}", "name"));

            modifierElement.Q<Label>("description").SetBinding("text", new LocalizedString($"BMTranslations{modifier.options.name}", "description"));

            modifierElement.Q<Button>("button").pickingMode = PickingMode.Ignore;
            modifierElement.Q<VisualElement>("icon").pickingMode = PickingMode.Ignore;
            modifierElement.Q<VisualElement>("textContainer").pickingMode = PickingMode.Ignore;
            modifierElement.Q<Label>("name").pickingMode = PickingMode.Ignore;
            modifierElement.Q<Label>("description").pickingMode = PickingMode.Ignore;

            scroll.Add(modifierElement);
        }

        statsContainer.RemoveFromClassList("turretChoiceMenuSlideOut");
    }
    void OnTowerBtnClicked(ClickEvent evt)
    {
        ClearList();

        //Creating List Elements for each Turretkind currently placed, giving them the turret they represent and a reference to the turret Detail Menu,
        //and adding the created Button to the scroll List
        foreach (var turret in GameManager.Instance.gameDataSO.GetSelectedBlueprints())
        {
            SM_TowerListElementBehavior towerElement = new SM_TowerListElementBehavior(turret, turretListElementAsset, this);
            uIDocument.rootVisualElement.Q("unity-content-container").Add(towerElement.listButton);
        }
    }

    //The Methods called and used for filling the turret Details
    public void FillTurretStatsDetails(TurretBlueprint _turret)
    {
        ClearStatsContaier();
        statsContainer.RemoveFromClassList("turretChoiceMenuSlideOut");

        //Creating the Turret Detail Menu
        TemplateContainer turretDetails = turretDetailsAsset.Instantiate();
        turretDetails.style.height = new Length(100, LengthUnit.Percent);
        statsContainer.Add(turretDetails);

        turretDetails.Q<Label>("name").SetBinding("text", new LocalizedString($"TurretTranslation{_turret.turretName}", $"name"));

        var turretIcon = turretDetails.Q<VisualElement>("icon");
        turretIcon.AddToClassList($"{_turret.turretName}Icon");

        turretDetails.Q<Label>($"costName").text = "";
        turretDetails.Q<Label>($"cost").text = $"";

        FillDetailValue("baseHealth", _turret.baseHealth, ref turretDetails);
        FillDetailValue("baseAttackDamage", _turret.baseAttackDamage, ref turretDetails);
        FillDetailValue("baseFireRate", _turret.baseFireRate, ref turretDetails);
        FillDetailValue("baseAttackRange", _turret.baseAttackRange, ref turretDetails);
        FillDetailValue("turretSize", _turret.height + _turret.length, ref turretDetails);
        FillDetailValue("placementCooldown", _turret.placementCooldown, ref turretDetails);
        FillDetailValue("buildCapacityValue", _turret.buildCapacityValue, ref turretDetails);
    }
    void FillDetailValue(string value, float turretValue, ref TemplateContainer _turretDetails)
    {
        var valueName = new Label();
        valueName.AddToClassList("paper-Text");
        valueName.SetBinding("text", new LocalizedString("TurretTranslationCommon", $"{value}"));
        _turretDetails.Q<VisualElement>("detailNames").Add(valueName);

        var valueNumbers = new Label();
        valueNumbers.AddToClassList("paper-Text");
        valueNumbers.text = $"{turretValue}";
        _turretDetails.Q<VisualElement>("detailNumbers").Add(valueNumbers);
    }

    void FillCharacterDetailValue(string value, float turretValue, ref TemplateContainer _turretDetails)
    {
        var valueName = new Label();
        valueName.AddToClassList("paper-Text");
        valueName.SetBinding("text", new LocalizedString("ClassTranslationCommon", $"{value}"));
        _turretDetails.Q<VisualElement>("detailNames").Add(valueName);

        var valueNumbers = new Label();
        valueNumbers.AddToClassList("paper-Text");
        valueNumbers.text = $"{turretValue}";
        _turretDetails.Q<VisualElement>("detailNumbers").Add(valueNumbers);
    }


    //Clearing Methods
    void ClearList()
    {
        //Ensuring that the Stats Container and scrollBox are empty, so that tower, charakter and relic details won't start overlapping
        uIDocument.rootVisualElement.Q("unity-content-container").Clear();
        ClearStatsContaier();
    }

    void ClearStatsContaier()
    {
        statsContainer.Clear();
        statsContainer.AddToClassList("turretChoiceMenuSlideOut");
    }
}
